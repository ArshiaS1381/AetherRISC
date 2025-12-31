using System;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.Pipeline;

namespace AetherRISC.Core.Architecture.Hardware.Pipeline.Stages
{
    public class ExecuteStage
    {
        private readonly MachineState _state;
        private readonly ArchitectureSettings _settings;

        public ExecuteStage(MachineState state, ArchitectureSettings settings)
        {
            _state = state;
            _settings = settings ?? new ArchitectureSettings();
        }

        public void Run(PipelineBuffers buffers)
        {
            if (!buffers.ExecuteMemory.IsStalled) 
                buffers.ExecuteMemory.Flush();

            if (buffers.DecodeExecute.IsEmpty || buffers.DecodeExecute.IsStalled) return;

            buffers.ExecuteMemory.SetHasContent();
            
            bool recoveryTriggered = false;
            bool stallDetected = false;
            ulong stallPC = 0;
            
            // Resource Counters for this cycle (Simulation of functional units)
            int usedIntALU = 0;
            int usedFPU = 0;
            int usedMem = 0;
            int usedBranch = 0;

            var inputs = buffers.DecodeExecute.Slots;
            var outputs = buffers.ExecuteMemory.Slots;
            var memWbSlots = buffers.MemoryWriteback.Slots;
            int width = buffers.Width;
            var regs = _state.Registers;

            for (int i = 0; i < width; i++)
            {
                var input = inputs[i];
                var output = outputs[i];

                // If a previous instruction in this bundle stalled or triggered recovery,
                // this instruction (and all subsequent) must become bubbles.
                if (stallDetected || recoveryTriggered || !input.Valid || input.IsBubble)
                {
                    output.Reset();
                    continue;
                }

                // --- RESOURCE CHECK (Structural Hazard) ---
                if (!CheckResources(input, ref usedIntALU, ref usedFPU, ref usedMem, ref usedBranch))
                {
                    // IN-ORDER CONSTRAINT:
                    // If we run out of ALUs for this instruction, we must stall THIS instruction
                    // AND all subsequent instructions in the bundle.
                    stallDetected = true;
                    stallPC = input.PC;
                    
                    // The instruction is not consumed. Logic below handles pipeline flush/rewind.
                    output.Reset();
                    continue; 
                }

                // --- DATA FORWARDING ---
                int rs1Idx = input.Rs1;
                int rs2Idx = input.Rs2;
                ulong rs1Val = regs.Read(rs1Idx);
                ulong rs2Val = regs.Read(rs2Idx);

                // Forward from WB (Youngest first)
                for (int k = width - 1; k >= 0; k--)
                {
                    var memOp = memWbSlots[k];
                    if (memOp.Valid && !memOp.IsBubble && memOp.RegWrite && memOp.Rd != 0)
                    {
                        if (memOp.Rd == rs1Idx) rs1Val = memOp.FinalResult;
                        if (memOp.Rd == rs2Idx) rs2Val = memOp.FinalResult;
                    }
                }

                // Cascaded Forwarding (Intra-stage)
                if (_settings.AllowCascadedExecution)
                {
                    for (int k = 0; k < i; k++)
                    {
                        var older = outputs[k]; 
                        if (older.Valid && !older.IsBubble && older.RegWrite && older.Rd != 0)
                        {
                            if (older.Rd == rs1Idx) rs1Val = older.AluResult;
                            if (older.Rd == rs2Idx) rs2Val = older.AluResult;
                        }
                    }
                }

                // ... (Load Reservation / Hazard Unit handles Load-Use stalls) ...

                if (input.ForwardedRs1.HasValue) rs1Val = input.ForwardedRs1.Value;
                if (input.ForwardedRs2.HasValue) rs2Val = input.ForwardedRs2.Value;

                // --- EXECUTION ---
                output.Valid = true;
                output.DecodedInst = input.DecodedInst;
                output.RawInstruction = input.RawInstruction;
                output.PC = input.PC;
                output.Rd = input.Rd;
                output.Immediate = input.Immediate;
                output.RegWrite = input.RegWrite;
                output.MemRead = input.MemRead;
                output.MemWrite = input.MemWrite;
                output.IsFloatRegWrite = input.IsFloatRegWrite;
                output.StoreValue = rs2Val;
                output.PredictedTaken = input.PredictedTaken;
                output.PredictedTarget = input.PredictedTarget;

                if (input.DecodedInst != null)
                {
                    input.DecodedInst.Compute(_state, rs1Val, rs2Val, output);

                    // Branch / Prediction Resolution
                    if (input.DecodedInst.IsBranch || input.DecodedInst.IsJump)
                    {
                        ulong instLen = ((input.RawInstruction & 0x3) == 0x3) ? 4u : 2u;
                        ulong fallthrough = input.PC + instLen;
                        
                        bool actualTaken = output.BranchTaken;
                        ulong correctNextPC = actualTaken ? output.ActualTarget : fallthrough;

                        if ((input.PredictedTaken != actualTaken) || (actualTaken && input.PredictedTarget != correctNextPC))
                        {
                            output.Misprediction = true;
                            output.CorrectTarget = correctNextPC;
                            _state.Registers.PC = correctNextPC;
                            _state.ProgramCounter = correctNextPC;
                            recoveryTriggered = true; // Kills subsequent instructions in this loop
                        }
                    }
                    else if (input.PredictedTaken)
                    {
                         // Spurious prediction on non-branch
                         ulong instLen = ((input.RawInstruction & 0x3) == 0x3) ? 4u : 2u;
                         ulong fallthrough = input.PC + instLen;
                         output.Misprediction = true;
                         output.CorrectTarget = fallthrough;
                         _state.Registers.PC = fallthrough;
                         _state.ProgramCounter = fallthrough;
                         recoveryTriggered = true;
                    }
                }
            }

            // --- HANDLING STALLS ---
            if (stallDetected)
            {
                // We ran out of execution ports.
                // 1. Reset PC to the instruction that stalled.
                _state.ProgramCounter = stallPC;
                
                // 2. Flush upstream buffers (Fetch/Decode) because they contain instructions
                //    that we are now going to re-fetch from the stall point.
                buffers.FetchDecode.Flush();
                buffers.DecodeExecute.Flush();
                
                // 3. Ensure Fetch isn't marked stalled (so it runs next cycle)
                buffers.FetchDecode.IsStalled = false;
                buffers.DecodeExecute.IsStalled = false;
                
                // Note: The valid instructions in outputs[0..stall_index-1] will proceed to Memory stage.
            }
        }

        private bool CheckResources(PipelineMicroOp op, ref int iALU, ref int fALU, ref int mem, ref int bru)
        {
            if (op.DecodedInst == null) return true; // NOP
            
            // Priority Check: Memory
            if ((op.MemRead || op.MemWrite) && _settings.MaxMemoryUnits > 0)
            {
                if (mem >= _settings.MaxMemoryUnits) return false;
                mem++;
                return true; 
            }

            // Branch Unit
            if ((op.DecodedInst.IsBranch || op.DecodedInst.IsJump) && _settings.MaxBranchUnits > 0)
            {
                if (bru >= _settings.MaxBranchUnits) return false;
                bru++;
                return true;
            }

            // Float Unit
            if (op.IsFloatRegWrite && _settings.MaxFloatALUs > 0)
            {
                if (fALU >= _settings.MaxFloatALUs) return false;
                fALU++;
                return true;
            }

            // Integer Unit (Default)
            if (_settings.MaxIntALUs > 0)
            {
                if (iALU >= _settings.MaxIntALUs) return false;
                iALU++;
            }
            
            return true;
        }
    }
}
