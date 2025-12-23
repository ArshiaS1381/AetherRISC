using AetherRISC.Core.Architecture;
using AetherRISC.Core.Architecture.ISA.Base;
using System;

namespace AetherRISC.Core.Architecture.ISA.Extensions.F
{
    public class FmvXWInstruction : Instruction {
        public override string Mnemonic => "FMV.X.W";
        public override int Rd { get; } public override int Rs1 { get; }
        public FmvXWInstruction(int rd, int rs1) { Rd = rd; Rs1 = rs1; }
        public override void Execute(MachineState s, InstructionData d) {
            uint bits = BitConverter.SingleToUInt32Bits(s.FRegisters.ReadSingle(d.Rs1));
            s.Registers.Write(d.Rd, (ulong)(long)(int)bits); // Sign-extend 32 to 64
        }
    }
}
