using AetherRISC.Core.Architecture;
namespace AetherRISC.Core.Architecture.ISA.Base {
    public class LhuInstruction : Instruction {
        public override string Mnemonic => "LHU";
        public override int Rd { get; } public override int Rs1 { get; } public override int Imm { get; }
        public LhuInstruction(int rd, int rs1, int imm) { Rd = rd; Rs1 = rs1; Imm = imm; }
        public override void Execute(MachineState s, InstructionData d) {
             long addr = (long)s.Registers.Read(d.Rs1) + (long)d.Immediate;
             // FIX: Use ReadHalf
             s.Registers.Write(d.Rd, (ulong)s.Memory!.ReadHalf((uint)addr)); // Zero-extend
        }
    }
}
