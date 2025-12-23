using System;
using AetherRISC.Core.Architecture;
using AetherRISC.Core.Architecture.ISA.Decoding;

namespace AetherRISC.Core.Architecture.System
{
    public class AetherRunner
    {
        private readonly MachineState _state;
        private readonly InstructionDecoder _decoder;

        public AetherRunner(MachineState state, InstructionDecoder decoder)
        {
            _state = state;
            _decoder = decoder;
        }

        public void ExecuteUntilHalt(int maxInstructions = 10000)
        {
            int count = 0;
            while (count < maxInstructions)
            {
                ulong currentPc = _state.ProgramCounter;
                
                // Fetch
                uint raw = _state.Memory!.ReadWord((uint)currentPc);
                
                // Decode
                var instruction = _decoder.Decode(raw);
                
                // Halt on EBREAK (0x00100073)
                if (raw == 0x00100073) break;

                // Create data with both Raw bits and the current PC
                var data = new InstructionData(raw, currentPc);

                // Execute
                instruction.Execute(_state, data); 
                
                // Advance PC if the instruction didn't perform a jump/branch
                if (_state.ProgramCounter == currentPc)
                {
                    _state.ProgramCounter += 4;
                }

                count++;
            }
        }
    }
}
