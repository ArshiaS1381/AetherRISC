using AetherRISC.Core.Architecture;
using AetherRISC.Core.Architecture.ISA.Base;

namespace AetherRISC.Core.Architecture.ISA.Extensions.M
{
    public class DivwInstruction : Instruction {
        public override string Mnemonic => "DIVW";
        public override int Rd { get; } public override int Rs1 { get; } public override int Rs2 { get; }
        public DivwInstruction(int rd, int rs1, int rs2) { Rd = rd; Rs1 = rs1; Rs2 = rs2; }
        public override void Execute(MachineState s, InstructionData d) {
            int v1 = (int)s.Registers.Read(d.Rs1);
            int v2 = (int)s.Registers.Read(d.Rs2);
            
            if (v2 == 0) s.Registers.Write(d.Rd, ulong.MaxValue); // -1 encoded
            else if (v1 == int.MinValue && v2 == -1) s.Registers.Write(d.Rd, unchecked((ulong)(long)int.MinValue));
            else s.Registers.Write(d.Rd, (ulong)(long)(v1 / v2));
        }
    }
}
