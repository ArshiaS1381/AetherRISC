using AetherRISC.Core.Architecture;
namespace AetherRISC.Core.Architecture.ISA.Base {
    public class SdInstruction : Instruction {
        public override string Mnemonic => "SD";
        public override int Rs1 { get; } public override int Rs2 { get; } public override int Imm { get; }
        public SdInstruction(int rs1, int rs2, int imm) { Rs1 = rs1; Rs2 = rs2; Imm = imm; }
        public override void Execute(MachineState s, InstructionData d) {
             long addr = (long)s.Registers.Read(d.Rs1) + (long)d.Immediate;
             ulong val = s.Registers.Read(d.Rs2);
             // FIX: Use WriteDouble (was WriteDoubleWord)
             s.Memory!.WriteDouble((uint)addr, val);
        }
    }
}
