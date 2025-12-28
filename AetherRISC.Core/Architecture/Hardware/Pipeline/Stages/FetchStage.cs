using System;
using AetherRISC.Core.Abstractions.Interfaces;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.Pipeline;

namespace AetherRISC.Core.Architecture.Hardware.Pipeline.Stages
{
    public class FetchStage
    {
        private readonly MachineState _state;

        public FetchStage(MachineState state)
        {
            _state = state;
        }

        public void Run(PipelineBuffers buffers)
        {
            if (buffers.FetchDecode.IsStalled) return;

            // FIX: Access PC via Registers
            ulong pc = _state.Registers.PC;
            
            // FIX: Cast ulong PC to uint for Memory.ReadWord
            uint instruction = _state.Memory!.ReadWord((uint)pc);

            buffers.FetchDecode.Instruction = instruction;
            buffers.FetchDecode.PC = pc;
            buffers.FetchDecode.IsValid = true;
            buffers.FetchDecode.IsEmpty = false;

            // FIX: Update PC via Registers
            _state.Registers.PC += 4;
        }
    }
}
