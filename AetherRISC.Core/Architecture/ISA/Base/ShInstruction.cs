using AetherRISC.Core.Architecture;
namespace AetherRISC.Core.Architecture.ISA.Base {
    public class ShInstruction : Instruction {
        public override string Mnemonic => "SH";
        public override int Rs1 { get; } public override int Rs2 { get; } public override int Imm { get; }
        public ShInstruction(int rs1, int rs2, int imm) { Rs1 = rs1; Rs2 = rs2; Imm = imm; }
        public override void Execute(MachineState s, InstructionData d) {
             long addr = (long)s.Registers.Read(d.Rs1) + (long)d.Immediate;
             ulong val = s.Registers.Read(d.Rs2);
             // FIX: Use WriteHalf (was WriteHalfWord)
             s.Memory!.WriteHalf((uint)addr, (ushort)(val & 0xFFFF));
        }
    }
}
