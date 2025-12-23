using AetherRISC.Core.Architecture;
namespace AetherRISC.Core.Architecture.ISA.Base {
    public class SbInstruction : Instruction {
        public override string Mnemonic => "SB";
        public override bool IsStore => true;
        public override int Rs1 { get; } public override int Rs2 { get; } public override int Imm { get; }
        public SbInstruction(int rs1, int rs2, int imm) { Rs1 = rs1; Rs2 = rs2; Imm = imm; }
        public override void Execute(MachineState s, InstructionData d) {
            ulong addr = (ulong)((long)s.Registers.Read(d.Rs1) + (long)d.Immediate);
            s.Memory!.WriteByte((uint)addr, (byte)s.Registers.Read(d.Rs2));
        }
    }
}
