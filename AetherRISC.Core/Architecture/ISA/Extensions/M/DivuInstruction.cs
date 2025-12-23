using AetherRISC.Core.Architecture;
using AetherRISC.Core.Architecture.ISA.Base;

namespace AetherRISC.Core.Architecture.ISA.Extensions.M
{
    public class DivuInstruction : Instruction {
        public override string Mnemonic => "DIVU";
        public override int Rd { get; } public override int Rs1 { get; } public override int Rs2 { get; }
        public DivuInstruction(int rd, int rs1, int rs2) { Rd = rd; Rs1 = rs1; Rs2 = rs2; }
        public override void Execute(MachineState s, InstructionData d) {
            ulong v1, v2;
            
            if (s.Config.XLEN == 32) {
                v1 = (uint)s.Registers.Read(d.Rs1);
                v2 = (uint)s.Registers.Read(d.Rs2);
            } else {
                v1 = s.Registers.Read(d.Rs1);
                v2 = s.Registers.Read(d.Rs2);
            }

            if (v2 == 0) s.Registers.Write(d.Rd, ulong.MaxValue);
            else s.Registers.Write(d.Rd, v1 / v2);
        }
    }
}
