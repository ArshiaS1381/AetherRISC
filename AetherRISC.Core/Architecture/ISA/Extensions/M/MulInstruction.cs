using AetherRISC.Core.Architecture;
using AetherRISC.Core.Architecture.ISA.Base;

namespace AetherRISC.Core.Architecture.ISA.Extensions.M
{
    public class MulInstruction : Instruction {
        public override string Mnemonic => "MUL";
        public override int Rd { get; } public override int Rs1 { get; } public override int Rs2 { get; }
        public MulInstruction(int rd, int rs1, int rs2) { Rd = rd; Rs1 = rs1; Rs2 = rs2; }
        public override void Execute(MachineState s, InstructionData d) {
            long v1 = (long)s.Registers.Read(d.Rs1);
            long v2 = (long)s.Registers.Read(d.Rs2);
            
            ulong res = unchecked((ulong)(v1 * v2));
            if (s.Config.XLEN == 32) res = (ulong)(uint)res; // Truncate to 32 bits
            
            s.Registers.Write(d.Rd, res);
        }
    }
}
