using AetherRISC.Core.Architecture;
using AetherRISC.Core.Architecture.ISA.Base;
using System;

namespace AetherRISC.Core.Architecture.ISA.Extensions.D
{
    public class FcvtDSInstruction : Instruction {
        public override string Mnemonic => "FcvtDS";
        public override int Rd { get; } public override int Rs1 { get; }
        public FcvtDSInstruction(int rd, int rs1) { Rd = rd; Rs1 = rs1; }
        
        public override void Execute(MachineState s, InstructionData d) {
            float v1 = s.FRegisters.ReadSingle(d.Rs1);
            double res = (double)((double)v1);
            s.FRegisters.WriteDouble(d.Rd, res);
        }
    }
}
