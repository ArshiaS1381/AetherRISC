using AetherRISC.Core.Architecture;
namespace AetherRISC.Core.Architecture.ISA.Base {
    public class NopInstruction : Instruction {
        public override string Mnemonic => "NOP";
        // Explicit constructor allows the Generator to create Inst.Nop()
        public NopInstruction() { } 
        public override void Execute(MachineState s, InstructionData d) { }
    }
}
