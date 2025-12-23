using AetherRISC.Core.Architecture;
using AetherRISC.Core.Architecture.ISA.Base;
using System;

namespace AetherRISC.Core.Architecture.ISA.Extensions.D
{
    public class FcvtDWInstruction : Instruction {
        public override string Mnemonic => "FcvtDW";
        public override int Rd { get; } public override int Rs1 { get; }
        public FcvtDWInstruction(int rd, int rs1) { Rd = rd; Rs1 = rs1; }
        
        public override void Execute(MachineState s, InstructionData d) {
            ulong v1 = s.Registers.Read(d.Rs1);
            double res = (double)((double)(int)v1);
            s.FRegisters.WriteDouble(d.Rd, res);
        }
    }
}
