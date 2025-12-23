using AetherRISC.Core.Architecture;
namespace AetherRISC.Core.Architecture.ISA.Base {
    public class LhInstruction : Instruction {
        public override string Mnemonic => "LH";
        public override int Rd { get; } public override int Rs1 { get; } public override int Imm { get; }
        public LhInstruction(int rd, int rs1, int imm) { Rd = rd; Rs1 = rs1; Imm = imm; }
        public override void Execute(MachineState s, InstructionData d) {
             long addr = (long)s.Registers.Read(d.Rs1) + (long)d.Immediate;
             // FIX: Use ReadHalf (was ReadHalfWord)
             ushort val = s.Memory!.ReadHalf((uint)addr);
             s.Registers.Write(d.Rd, (ulong)(long)(short)val); // Sign-extend 16->64
        }
    }
}
