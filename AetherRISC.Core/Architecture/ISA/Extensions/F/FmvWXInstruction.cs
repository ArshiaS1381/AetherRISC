using AetherRISC.Core.Architecture;
using AetherRISC.Core.Architecture.ISA.Base;
using System;

namespace AetherRISC.Core.Architecture.ISA.Extensions.F
{
    public class FmvWXInstruction : Instruction {
        public override string Mnemonic => "FMV.W.X";
        public override int Rd { get; } public override int Rs1 { get; }
        public FmvWXInstruction(int rd, int rs1) { Rd = rd; Rs1 = rs1; }
        public override void Execute(MachineState s, InstructionData d) {
            uint bits = (uint)(s.Registers.Read(d.Rs1) & 0xFFFFFFFF);
            s.FRegisters.WriteSingle(d.Rd, bits); // This handles NaN-boxing internally
        }
    }
}
