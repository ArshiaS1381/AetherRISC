using AetherRISC.Core.Architecture;
using AetherRISC.Core.Architecture.ISA.Base;

namespace AetherRISC.Core.Architecture.ISA.Extensions.Zifencei
{
    public class FenceIInstruction : Instruction {
        public override string Mnemonic => "FENCE.I";
        public FenceIInstruction() { }
        public override void Execute(MachineState s, InstructionData d) {
            // No-op for emulator, but required for architecture compliance
        }
    }
}
