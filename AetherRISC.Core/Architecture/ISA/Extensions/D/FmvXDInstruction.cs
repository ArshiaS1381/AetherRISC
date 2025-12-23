using AetherRISC.Core.Architecture;
using AetherRISC.Core.Architecture.ISA.Base;

namespace AetherRISC.Core.Architecture.ISA.Extensions.D
{
    public class FmvXDInstruction : Instruction {
        public override string Mnemonic => "FMV.X.D";
        public override int Rd { get; } public override int Rs1 { get; }
        public FmvXDInstruction(int rd, int rs1) { Rd = rd; Rs1 = rs1; }
        public override void Execute(MachineState s, InstructionData d) {
            // Directly copy the raw 64-bit pattern
            s.Registers.Write(d.Rd, s.FRegisters.Read(d.Rs1));
        }
    }
}
