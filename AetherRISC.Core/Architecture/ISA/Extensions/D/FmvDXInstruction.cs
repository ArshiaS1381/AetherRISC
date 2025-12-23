using AetherRISC.Core.Architecture;
using AetherRISC.Core.Architecture.ISA.Base;

namespace AetherRISC.Core.Architecture.ISA.Extensions.D
{
    public class FmvDXInstruction : Instruction {
        public override string Mnemonic => "FMV.D.X";
        public override int Rd { get; } public override int Rs1 { get; }
        public FmvDXInstruction(int rd, int rs1) { Rd = rd; Rs1 = rs1; }
        public override void Execute(MachineState s, InstructionData d) {
            // Directly copy the raw 64-bit pattern
            s.FRegisters.Write(d.Rd, s.Registers.Read(d.Rs1));
        }
    }
}
