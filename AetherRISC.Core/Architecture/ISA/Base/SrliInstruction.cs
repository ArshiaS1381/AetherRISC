using AetherRISC.Core.Architecture;
namespace AetherRISC.Core.Architecture.ISA.Base {
    public class SrliInstruction : Instruction {
        public override string Mnemonic => "SRLI";
        public override int Rd { get; } public override int Rs1 { get; } public override int Imm { get; }
        public SrliInstruction(int rd, int rs1, int imm) { Rd = rd; Rs1 = rs1; Imm = imm; }
        public override void Execute(MachineState s, InstructionData d) => s.Registers.Write(d.Rd, s.Registers.Read(d.Rs1) >> (int)(d.Immediate & 0x3F));
    }
}
