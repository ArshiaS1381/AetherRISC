using System;
using AetherRISC.Core.Abstractions.Interfaces;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.Pipeline;

namespace AetherRISC.Core.Architecture.Hardware.Pipeline.Stages
{
    public class MemoryStage
    {
        private readonly MachineState _state;

        public MemoryStage(MachineState state)
        {
            _state = state;
        }

        public void Run(PipelineBuffers buffers)
        {
            if (buffers.ExecuteMemory.IsEmpty)
            {
                buffers.MemoryWriteback.IsEmpty = true;
                return;
            }

            var input = buffers.ExecuteMemory;
            var output = buffers.MemoryWriteback;

            output.DecodedInst = input.DecodedInst;
            output.RawInstruction = input.RawInstruction;
            output.PC = input.PC;
            output.Rd = input.Rd;
            output.RegWrite = input.RegWrite;

            ulong result = input.AluResult;

            if (input.MemRead)
            {
                // LOAD: Read from memory
                result = _state.Memory.ReadWord((uint)input.AluResult); 
            }
            else if (input.MemWrite)
            {
                // STORE: Write StoreValue to memory
                _state.Memory?.WriteWord((uint)input.AluResult, (uint)input.StoreValue);
            }
            
            output.FinalResult = result;
            output.IsEmpty = false;
        }
    }
}
