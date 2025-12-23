using AetherRISC.Core.Architecture;
namespace AetherRISC.Core.Architecture.ISA.Base {
    public class AddiInstruction : Instruction {
        public override string Mnemonic => "ADDI";
        public override int Rd { get; } public override int Rs1 { get; } public override int Imm { get; }
        public AddiInstruction(int rd, int rs1, int imm) { Rd = rd; Rs1 = rs1; Imm = imm; }
        public override void Execute(MachineState s, InstructionData d) {
            long res = (long)s.Registers.Read(d.Rs1) + (long)d.Immediate;
            if (s.Config.XLEN == 32) s.Registers.Write(d.Rd, (ulong)(uint)res);
            else s.Registers.Write(d.Rd, (ulong)res);
        }
    }
}
