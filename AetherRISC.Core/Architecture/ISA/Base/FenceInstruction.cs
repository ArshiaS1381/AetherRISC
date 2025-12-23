using AetherRISC.Core.Architecture;
namespace AetherRISC.Core.Architecture.ISA.Base {
    public class FenceInstruction : Instruction {
        public override string Mnemonic => "FENCE";
        public override void Execute(MachineState s, InstructionData d) { } // No-op in simple emulator
    }
}
