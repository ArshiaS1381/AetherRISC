using AetherRISC.Core.Architecture;
using AetherRISC.Core.Architecture.ISA.Base;
using System;

namespace AetherRISC.Core.Architecture.ISA.Extensions.F
{
    public class FcvtSWInstruction : Instruction {
        public override string Mnemonic => "FcvtSW";
        public override int Rd { get; } public override int Rs1 { get; }
        public FcvtSWInstruction(int rd, int rs1) { Rd = rd; Rs1 = rs1; }
        
        public override void Execute(MachineState s, InstructionData d) {
            ulong v1 = s.Registers.Read(d.Rs1);
            float res = (float)((float)(int)v1);
            s.FRegisters.WriteSingle(d.Rd, res);
        }
    }
}
