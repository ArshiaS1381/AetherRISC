using System;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.Pipeline;
using AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RvSystem;

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

            for (int i = 0; i < buffers.Width; i++)
            {
                var input = buffers.MemoryWriteback.Slots[i];
                
                // Skip empty slots
                if (!input.Valid || input.IsBubble) continue;

                // CRITICAL FIX: Shadow Execution Prevention
                // If a previous instruction in this bundle (or external event) halted the machine,
                // we MUST invalidate this slot so it doesn't commit or get counted.
                if (_state.Halted)
                {
                    input.Reset();
                    continue;
                }

                if (input.RegWrite)
                {
                    if (input.IsFloatRegWrite)
                    {
                        _state.FRegisters.WriteDouble(input.Rd, BitConverter.UInt64BitsToDouble(input.FinalResult));
                    }
                    else if (input.Rd != 0)
                    {
                        _state.Registers[input.Rd] = input.FinalResult;
                    }
                }

                // Check for Halt *after* processing registers? 
                // EBREAK usually doesn't write registers, but logic holds.
                if (input.DecodedInst is EbreakInstruction) _state.Halted = true;
                else if (input.DecodedInst is EcallInstruction) _state.Host?.HandleEcall(_state);
            }
        }
    }
}
