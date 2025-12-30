using System;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.Pipeline;
using AetherRISC.Core.Architecture.Hardware.ISA.Decoding;

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
            {
                buffers.ExecuteMemory.Flush();
            }

            if (buffers.DecodeExecute.IsEmpty || buffers.DecodeExecute.IsStalled) return;

            buffers.ExecuteMemory.SetHasContent();
            
            bool recoveryTriggered = false;
            var inputs = buffers.DecodeExecute.Slots;
            var outputs = buffers.ExecuteMemory.Slots;
            var memWbSlots = buffers.MemoryWriteback.Slots;
            int width = buffers.Width;
            var regs = _state.Registers;

            // Resource Counters for this cycle
            int usedIntALU = 0;
            int usedFPU = 0;
            int usedMem = 0;
            int usedBranch = 0;
            
            bool stallDetected = false;
            ulong stallPC = 0;

            for (int i = 0; i < width; i++)
            {
                var input = inputs[i];
                var output = outputs[i];

                if (stallDetected || recoveryTriggered || !input.Valid || input.IsBubble)
                {
                    output.Reset();
                    continue;
                }

                // --- RESOURCE CHECK START ---
                bool hasResource = CheckResources(input, ref usedIntALU, ref usedFPU, ref usedMem, ref usedBranch);
                if (!hasResource)
                {
                    // Resource exhaustion.
                    // Since this is in-order issue, we must stall THIS instruction and all newer ones.
                    // We treat this like a hazard: Stall Fetch/Decode, Flush this slot.
                    stallDetected = true;
                    stallPC = input.PC;
                    
                    // We must tell the upstream stage (Decode) that we did NOT consume this instruction.
                    // However, 'DecodeStage' has already advanced. 
                    // This requires a "Replay" mechanism similar to DataHazards.
                    // For simplicity here, we simulate it by setting the Pipeline Stalls.
                    
                    // Reset this output (it becomes a bubble)
                    output.Reset();
                    continue; 
                }
                // --- RESOURCE CHECK END ---

                int rs1Idx = input.Rs1;
                int rs2Idx = input.Rs2;

                ulong rs1Val = regs.Read(rs1Idx);
                ulong rs2Val = regs.Read(rs2Idx);

                // Forwarding from WB stage
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

                if (input.ForwardedRs1.HasValue) rs1Val = input.ForwardedRs1.Value;
                if (input.ForwardedRs2.HasValue) rs2Val = input.ForwardedRs2.Value;

                // Propagate to Output
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

                    // Branch Resolution Logic
                    if (input.DecodedInst.IsBranch || input.DecodedInst.IsJump)
                    {
                        ulong instLen = ((input.RawInstruction & 0x3) == 0x3) ? 4u : 2u;
                        ulong fallthrough = input.PC + instLen;
                        
                        bool actualTaken = output.BranchTaken;
                        ulong correctNextPC = actualTaken ? output.ActualTarget : fallthrough;

                        bool predTaken = input.PredictedTaken;
                        ulong predTarget = input.PredictedTarget;
                        
                        if ((predTaken != actualTaken) || (actualTaken && predTarget != correctNextPC))
                        {
                            output.Misprediction = true;
                            output.CorrectTarget = correctNextPC;
                            _state.Registers.PC = correctNextPC;
                            _state.ProgramCounter = correctNextPC;
                            recoveryTriggered = true;
                        }
                    }
                    else if (input.PredictedTaken)
                    {
                         // Was predicted taken, but is not a branch/jump instruction? (Spurious prediction)
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

            if (stallDetected)
            {
                // We couldn't issue the full bundle due to resources.
                // We need to replay starting from the stalled PC.
                _state.ProgramCounter = stallPC;
                
                // Flush upstream to force re-fetch/decode of the stalled instructions
                buffers.FetchDecode.Flush();
                buffers.DecodeExecute.Flush();
                
                // Ensure Fetch isn't stalled so it can grab the "replay" PC immediately
                buffers.FetchDecode.IsStalled = false;
            }
        }

        private bool CheckResources(PipelineMicroOp op, ref int iALU, ref int fALU, ref int mem, ref int bru)
        {
            if (op.DecodedInst == null) return true; // NOP usually consumes nothing or just iALU
            
            // 1. Memory
            if ((op.MemRead || op.MemWrite) && _settings.MaxMemoryUnits > 0)
            {
                if (mem >= _settings.MaxMemoryUnits) return false;
                mem++;
                return true; 
            }

            // 2. Branch
            if ((op.DecodedInst.IsBranch || op.DecodedInst.IsJump) && _settings.MaxBranchUnits > 0)
            {
                if (bru >= _settings.MaxBranchUnits) return false;
                bru++;
                return true;
            }

            // 3. Float
            // We need to check if the instruction is floating point. 
            // Currently simplified check: IsFloatRegWrite is a strong hint, but some FP ops (FCLASS) write to Int.
            // Ideally, add IsFloat property to IInstruction. Assuming IsFloatRegWrite covers most compute.
            if (op.IsFloatRegWrite && _settings.MaxFloatALUs > 0)
            {
                if (fALU >= _settings.MaxFloatALUs) return false;
                fALU++;
                return true;
            }

            // 4. Integer (Default fallthrough)
            if (_settings.MaxIntALUs > 0)
            {
                if (iALU >= _settings.MaxIntALUs) return false;
                iALU++;
            }
            
            return true;
        }
    }
}
