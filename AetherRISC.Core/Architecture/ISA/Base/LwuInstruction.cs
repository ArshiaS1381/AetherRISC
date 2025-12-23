using AetherRISC.Core.Architecture;
namespace AetherRISC.Core.Architecture.ISA.Base {
    public class LwuInstruction : Instruction {
        public override string Mnemonic => "LWU";
        public override bool IsLoad => true;
        public override int Rd { get; } public override int Rs1 { get; } public override int Imm { get; }
        public LwuInstruction(int rd, int rs1, int imm) { Rd = rd; Rs1 = rs1; Imm = imm; }
        public override void Execute(MachineState s, InstructionData d) {
            ulong addr = (ulong)((long)s.Registers.Read(d.Rs1) + (long)d.Immediate);
            uint val = s.Memory!.ReadWord((uint)addr); // Zero Extend to 64-bit
            s.Registers.Write(d.Rd, (ulong)val);
        }
    }
}
