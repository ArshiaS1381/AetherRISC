using AetherRISC.Core.Architecture;
using AetherRISC.Core.Architecture.ISA.Base;
using System;

namespace AetherRISC.Core.Architecture.ISA.Extensions.F
{
    public class FcvtWSInstruction : Instruction {
        public override string Mnemonic => "FcvtWS";
        public override int Rd { get; } public override int Rs1 { get; }
        public FcvtWSInstruction(int rd, int rs1) { Rd = rd; Rs1 = rs1; }
        
        public override void Execute(MachineState s, InstructionData d) {
            float v1 = s.FRegisters.ReadSingle(d.Rs1);
            ulong res = (ulong)((int)v1);
            s.Registers.Write(d.Rd, res);
        }
    }
}
