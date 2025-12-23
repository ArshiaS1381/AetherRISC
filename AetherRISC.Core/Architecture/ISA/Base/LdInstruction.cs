using AetherRISC.Core.Architecture;
namespace AetherRISC.Core.Architecture.ISA.Base {
    public class LdInstruction : Instruction {
        public override string Mnemonic => "LD";
        public override int Rd { get; } public override int Rs1 { get; } public override int Imm { get; }
        public LdInstruction(int rd, int rs1, int imm) { Rd = rd; Rs1 = rs1; Imm = imm; }
        public override void Execute(MachineState s, InstructionData d) {
             long addr = (long)s.Registers.Read(d.Rs1) + (long)d.Immediate;
             // FIX: Use ReadDouble (was ReadDoubleWord)
             s.Registers.Write(d.Rd, s.Memory!.ReadDouble((uint)addr));
        }
    }
}
