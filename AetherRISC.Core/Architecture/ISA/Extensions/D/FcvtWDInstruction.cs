using AetherRISC.Core.Architecture;
using AetherRISC.Core.Architecture.ISA.Base;
using System;

namespace AetherRISC.Core.Architecture.ISA.Extensions.D
{
    public class FcvtWDInstruction : Instruction {
        public override string Mnemonic => "FcvtWD";
        public override int Rd { get; } public override int Rs1 { get; }
        public FcvtWDInstruction(int rd, int rs1) { Rd = rd; Rs1 = rs1; }
        
        public override void Execute(MachineState s, InstructionData d) {
            double v1 = s.FRegisters.ReadDouble(d.Rs1);
            ulong res = (ulong)((int)v1);
            s.Registers.Write(d.Rd, res);
        }
    }
}
