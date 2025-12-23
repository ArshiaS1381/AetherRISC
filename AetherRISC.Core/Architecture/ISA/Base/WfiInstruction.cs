using AetherRISC.Core.Architecture;
using AetherRISC.Core.Architecture.ISA.Base;

namespace AetherRISC.Core.Architecture.ISA.Base
{
    public class WfiInstruction : Instruction {
        public override string Mnemonic => "WFI";
        public override void Execute(MachineState s, InstructionData d) {
            // In an emulator, this is often a NOP or a small thread sleep
            // until an external interrupt is signaled.
        }
    }
}
