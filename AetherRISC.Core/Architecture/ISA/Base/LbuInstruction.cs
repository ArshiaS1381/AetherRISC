using AetherRISC.Core.Architecture;
namespace AetherRISC.Core.Architecture.ISA.Base {
    public class LbuInstruction : Instruction {
        public override string Mnemonic => "LBU";
        public override bool IsLoad => true;
        public override int Rd { get; } public override int Rs1 { get; } public override int Imm { get; }
        public LbuInstruction(int rd, int rs1, int imm) { Rd = rd; Rs1 = rs1; Imm = imm; }
        public override void Execute(MachineState s, InstructionData d) {
            ulong addr = (ulong)((long)s.Registers.Read(d.Rs1) + (long)d.Immediate);
            byte val = s.Memory!.ReadByte((uint)addr); // Zero Extend
            s.Registers.Write(d.Rd, (ulong)val);
        }
    }
}
