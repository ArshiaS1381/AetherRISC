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
            
            var inputs = buffers.DecodeExecute.Slots;
            var outputs = buffers.ExecuteMemory.Slots;
            var memWbSlots = buffers.MemoryWriteback.Slots;
            int width = buffers.Width;
            var regs = _state.Registers;

            for (int i = 0; i < width; i++)
            {
                var input = inputs[i];
                var output = outputs[i];

                if (recoveryTriggered || !input.Valid || input.IsBubble)
                {
                    output.Reset();
                    continue;
                }

                // Structural Hazards handled in StructuralHazardUnit now.
                // Data Hazards (Stalls) handled in DataHazardUnit.
                
                // --- DATA FORWARDING (Operand Retrieval) ---
                // 1. RegFile Read
                ulong rs1Val = regs.Read(input.Rs1);
                ulong rs2Val = regs.Read(input.Rs2);

                // 2. Forward from Writeback (Older stages)
                for (int k = width - 1; k >= 0; k--)
                {
                    var memOp = memWbSlots[k];
                    if (memOp.Valid && !memOp.IsBubble && memOp.RegWrite && memOp.Rd != 0)
                    {
                        if (memOp.Rd == input.Rs1) rs1Val = memOp.FinalResult;
                        if (memOp.Rd == input.Rs2) rs2Val = memOp.FinalResult;
                    }
                }

                // 3. Intra-stage Forwarding (Cascaded Execution)
                if (_settings.AllowCascadedExecution)
                {
                    for (int k = 0; k < i; k++)
                    {
                        var older = outputs[k]; 
                        if (older.Valid && !older.IsBubble && older.RegWrite && older.Rd != 0)
                        {
                            if (older.Rd == input.Rs1) rs1Val = older.AluResult;
                            if (older.Rd == input.Rs2) rs2Val = older.AluResult;
                        }
                    }
                }

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

                    // Branch Verification
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
                            _state.ProgramCounter = correctNextPC; // Fix PC immediately for simple cases
                            recoveryTriggered = true; 
                        }
                    }
                }
            }
        }
    }
}
