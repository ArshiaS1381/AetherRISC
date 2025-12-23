using AetherRISC.Core.Architecture;
namespace AetherRISC.Core.Architecture.ISA.Base {
    public class SltiInstruction : Instruction {
        public override string Mnemonic => "SLTI";
        public override int Rd { get; } public override int Rs1 { get; } public override int Imm { get; }
        public SltiInstruction(int rd, int rs1, int imm) { Rd = rd; Rs1 = rs1; Imm = imm; }
        public override void Execute(MachineState s, InstructionData d) => s.Registers.Write(d.Rd, (long)s.Registers.Read(d.Rs1) < (long)d.Immediate ? 1ul : 0ul);
    }
}
