using AetherRISC.Core.Architecture;
using AetherRISC.Core.Architecture.ISA.Base;
using System;

namespace AetherRISC.Core.Architecture.ISA.Extensions.F
{
    public class FsqrtSInstruction : Instruction {
        public override string Mnemonic => "FsqrtS";
        public override int Rd { get; } public override int Rs1 { get; }
        public FsqrtSInstruction(int rd, int rs1) { Rd = rd; Rs1 = rs1; }
        
        public override void Execute(MachineState s, InstructionData d) {
            float v1 = s.FRegisters.ReadSingle(d.Rs1);
            float res = (float)(Math.Sqrt(v1));
            s.FRegisters.WriteSingle(d.Rd, res);
        }
    }
}
