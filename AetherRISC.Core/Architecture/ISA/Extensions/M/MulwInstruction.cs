using AetherRISC.Core.Architecture;
using AetherRISC.Core.Architecture.ISA.Base;

namespace AetherRISC.Core.Architecture.ISA.Extensions.M
{
    public class MulwInstruction : Instruction {
        public override string Mnemonic => "MULW";
        public override int Rd { get; } public override int Rs1 { get; } public override int Rs2 { get; }
        public MulwInstruction(int rd, int rs1, int rs2) { Rd = rd; Rs1 = rs1; Rs2 = rs2; }
        public override void Execute(MachineState s, InstructionData d) {
            // Multiply low 32 bits, result is 32-bit signed (sign-extended to 64)
            int v1 = (int)s.Registers.Read(d.Rs1);
            int v2 = (int)s.Registers.Read(d.Rs2);
            s.Registers.Write(d.Rd, (ulong)(long)(v1 * v2));
        }
    }
}
