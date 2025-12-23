using AetherRISC.Core.Architecture;
using AetherRISC.Core.Architecture.ISA.Base;
using System;

namespace AetherRISC.Core.Architecture.ISA.Extensions.D
{
    public class FltDInstruction : Instruction {
        public override string Mnemonic => "FltD";
        public override int Rd { get; } public override int Rs1 { get; } public override int Rs2 { get; }
        public FltDInstruction(int rd, int rs1, int rs2) { Rd = rd; Rs1 = rs1; Rs2 = rs2; }
        
        public override void Execute(MachineState s, InstructionData d) {
            double v1 = s.FRegisters.ReadDouble(d.Rs1);
            double v2 = s.FRegisters.ReadDouble(d.Rs2);
            bool res = v1 < v2;
            s.Registers.Write(d.Rd, res ? 1u : 0u);
        }
    }
}
