using AetherRISC.Core.Architecture;
using AetherRISC.Core.Architecture.ISA.Base;
namespace AetherRISC.Core.Architecture.ISA.Extensions.D
{
    public class FcvtLDInstruction : Instruction {
        public override string Mnemonic => "FCVT.L.D";
        public override int Rd { get; } public override int Rs1 { get; }
        public FcvtLDInstruction(int rd, int rs1) { Rd = rd; Rs1 = rs1; }
        public override void Execute(MachineState s, InstructionData d) {
            double val = s.FRegisters.ReadDouble(d.Rs1);
            s.Registers.Write(d.Rd, (ulong)(long)val);
        }
    }
}
