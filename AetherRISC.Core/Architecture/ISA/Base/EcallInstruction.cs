using AetherRISC.Core.Architecture;
namespace AetherRISC.Core.Architecture.ISA.Base {
    public class EcallInstruction : Instruction {
        public override string Mnemonic => "ECALL";
        // Explicit constructor allows the Generator to create Inst.Ecall()
        public EcallInstruction() { }
        public override void Execute(MachineState s, InstructionData d) { 
             s.Host?.PrintString("ECALL Triggered");
        }
    }
}
