using AetherRISC.Core.Architecture;
using AetherRISC.Core.Architecture.ISA.Base;

namespace AetherRISC.Core.Architecture.ISA.Extensions.F
{
    public class FswInstruction : Instruction {
        public override string Mnemonic => "FSW";
        public override int Rs1 { get; } public override int Rs2 { get; } public override int Imm { get; }
        
        public FswInstruction(int rs1, int rs2, int imm) { Rs1 = rs1; Rs2 = rs2; Imm = imm; }
        
        public override void Execute(MachineState s, InstructionData d) {
             long addr = (long)s.Registers.Read(d.Rs1) + (long)d.Immediate;
             ulong raw = s.FRegisters.Read(d.Rs2); // Read F-Register
             s.Memory!.WriteWord((uint)addr, (uint)(raw & 0xFFFFFFFF));
        }
    }
}
