using AetherRISC.Core.Architecture;
namespace AetherRISC.Core.Architecture.ISA.Base {
    public class XoriInstruction : Instruction {
        public override string Mnemonic => "XORI";
        public override int Rd { get; } public override int Rs1 { get; } public override int Imm { get; }
        public XoriInstruction(int rd, int rs1, int imm) { Rd = rd; Rs1 = rs1; Imm = imm; }
        public override void Execute(MachineState s, InstructionData d) => s.Registers.Write(d.Rd, s.Registers.Read(d.Rs1) ^ (ulong)(long)d.Immediate);
    }
}
