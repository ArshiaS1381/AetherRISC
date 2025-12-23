using AetherRISC.Core.Architecture;
using AetherRISC.Core.Architecture.ISA.Base;
using System;

namespace AetherRISC.Core.Architecture.ISA.Extensions.F
{
    public class FaddSInstruction : Instruction {
        public override string Mnemonic => "FaddS";
        public override int Rd { get; } public override int Rs1 { get; } public override int Rs2 { get; }
        public FaddSInstruction(int rd, int rs1, int rs2) { Rd = rd; Rs1 = rs1; Rs2 = rs2; }
        
        public override void Execute(MachineState s, InstructionData d) {
            float v1 = s.FRegisters.ReadSingle(d.Rs1);
            float v2 = s.FRegisters.ReadSingle(d.Rs2);
            float res = v1 + v2;
            s.FRegisters.WriteSingle(d.Rd, res);
        }
    }
}
