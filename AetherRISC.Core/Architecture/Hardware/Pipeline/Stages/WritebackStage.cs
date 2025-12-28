using System;
using AetherRISC.Core.Abstractions.Interfaces;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.Pipeline;
using AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RvSystem; // Required for type checking

namespace AetherRISC.Core.Architecture.Hardware.Pipeline.Stages
{
    public class WritebackStage
    {
        private readonly MachineState _state;

        public WritebackStage(MachineState state)
        {
            _state = state;
        }

        public void Run(PipelineBuffers buffers)
        {
            if (buffers.MemoryWriteback.IsEmpty) return;

            var input = buffers.MemoryWriteback;

            // 1. Write to Register File
            if (input.RegWrite && input.Rd != 0)
            {
                _state.Registers[input.Rd] = input.FinalResult;
            }
            
            // 2. Handle System Instructions (Commit Point)
            if (input.DecodedInst is EbreakInstruction)
            {
                _state.Halted = true;
            }
            else if (input.DecodedInst is EcallInstruction)
            {
                _state.Host?.HandleEcall(_state);
            }
        }
    }
}
