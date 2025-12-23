using AetherRISC.Core.Architecture;
using AetherRISC.Core.Architecture.ISA.Base;

namespace AetherRISC.Core.Architecture.ISA.Extensions.F
{
    public class FlwInstruction : Instruction {
        public override string Mnemonic => "FLW";
        public override int Rd { get; } public override int Rs1 { get; } public override int Imm { get; }
        
        public FlwInstruction(int rd, int rs1, int imm) { Rd = rd; Rs1 = rs1; Imm = imm; }
        
        public override void Execute(MachineState s, InstructionData d) {
             long addr = (long)s.Registers.Read(d.Rs1) + (long)d.Immediate;
             uint val = s.Memory!.ReadWord((uint)addr);
             
             // Write to F-Register (Rd) using Single Precision logic
             s.FRegisters.WriteSingle(d.Rd, val);
        }
    }
}
