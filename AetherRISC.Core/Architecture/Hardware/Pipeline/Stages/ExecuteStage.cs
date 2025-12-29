using System;
using AetherRISC.Core.Abstractions.Interfaces;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.Pipeline;
using AetherRISC.Core.Architecture.Hardware.ISA.Decoding;

namespace AetherRISC.Core.Architecture.Hardware.Pipeline.Stages
{
    public class ExecuteStage
    {
        private readonly MachineState _state;

        public ExecuteStage(MachineState state)
        {
            _state = state;
        }

        public void Run(PipelineBuffers buffers)
        {
            if (buffers.DecodeExecute.IsEmpty) 
            {
                buffers.ExecuteMemory.Flush();
                return;
            }

            var input = buffers.DecodeExecute;
            var output = buffers.ExecuteMemory;

            // [SNAPSHOT] Capture where the Fetch Unit currently is
            ulong speculativeFetchPC = _state.Registers.PC;

            ulong rs1Val = input.ForwardedRs1 ?? _state.Registers[input.DecodedInst?.Rs1 ?? 0];
            ulong rs2Val = input.ForwardedRs2 ?? _state.Registers[input.DecodedInst?.Rs2 ?? 0];

            output.DecodedInst = input.DecodedInst;
            output.RawInstruction = input.RawInstruction;
            output.PC = input.PC;
            output.Rd = input.Rd;
            output.RegWrite = input.RegWrite;
            output.MemRead = input.MemRead;
            output.MemWrite = input.MemWrite;
            output.StoreValue = rs2Val;
            output.BranchTaken = false; 
            
            output.Misprediction = false;
            output.CorrectTarget = 0;
            output.ActualTarget = 0;

            if (input.DecodedInst != null)
            {
                input.DecodedInst.Compute(_state, rs1Val, rs2Val, buffers);
                
                bool isControlFlow = input.DecodedInst.IsBranch || input.DecodedInst.IsJump;
                ulong fallthrough = input.PC + (InstructionDecompressor.IsCompressed((ushort)input.RawInstruction) ? 2u : 4u);
                
                bool actualTaken = output.BranchTaken;
                ulong actualTarget = 0;

                if (isControlFlow)
                {
                    if (actualTaken) actualTarget = _state.Registers.PC;
                    
                    ulong correctNextPC = actualTaken ? actualTarget : fallthrough;
                    output.ActualTarget = actualTarget;

                    bool predTaken = input.PredictedTaken;
                    ulong predTarget = input.PredictedTarget;

                    bool mispredicted = (predTaken != actualTaken) || (actualTaken && predTarget != actualTarget);

                    if (mispredicted)
                    {
                        output.Misprediction = true;
                        output.CorrectTarget = correctNextPC;
                        _state.Registers.PC = correctNextPC;
                    }
                    else
                    {
                        // Restore speculatively correct PC
                        _state.Registers.PC = speculativeFetchPC;
                    }
                }
                else
                {
                    if (input.PredictedTaken)
                    {
                        output.Misprediction = true;
                        output.CorrectTarget = fallthrough;
                        _state.Registers.PC = fallthrough;
                    }
                    else
                    {
                        _state.Registers.PC = speculativeFetchPC;
                    }
                }
            }

            output.IsEmpty = false;
        }
    }
}
