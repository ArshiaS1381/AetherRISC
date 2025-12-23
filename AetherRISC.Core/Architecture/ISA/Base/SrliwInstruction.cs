using AetherRISC.Core.Architecture;
namespace AetherRISC.Core.Architecture.ISA.Base {
    public class SrliwInstruction : Instruction {
        public override string Mnemonic => "SRLIW";
        public override int Rd { get; } public override int Rs1 { get; } public override int Imm { get; }
        public SrliwInstruction(int rd, int rs1, int imm) { Rd = rd; Rs1 = rs1; Imm = imm; }
        public override void Execute(MachineState s, InstructionData d) {
            uint v1 = (uint)s.Registers.Read(d.Rs1);
            int shamt = (int)d.Immediate & 0x1F;
            s.Registers.Write(d.Rd, (ulong)(long)(int)(v1 >> shamt));
        }
    }
}
