using AetherRISC.Core.Architecture;
namespace AetherRISC.Core.Architecture.ISA.Base {
    public class SraiInstruction : Instruction {
        public override string Mnemonic => "SRAI";
        public override int Rd { get; } public override int Rs1 { get; } public override int Imm { get; }
        public SraiInstruction(int rd, int rs1, int imm) { Rd = rd; Rs1 = rs1; Imm = imm; }
        public override void Execute(MachineState s, InstructionData d) => s.Registers.Write(d.Rd, (ulong)((long)s.Registers.Read(d.Rs1) >> (int)(d.Immediate & 0x3F)));
    }
}
