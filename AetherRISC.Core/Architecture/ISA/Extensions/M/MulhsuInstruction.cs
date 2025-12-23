using AetherRISC.Core.Architecture;
using AetherRISC.Core.Architecture.ISA.Base;
using System;

namespace AetherRISC.Core.Architecture.ISA.Extensions.M
{
    public class MulhsuInstruction : Instruction {
        public override string Mnemonic => "MULHSU";
        public override int Rd { get; } public override int Rs1 { get; } public override int Rs2 { get; }
        public MulhsuInstruction(int rd, int rs1, int rs2) { Rd = rd; Rs1 = rs1; Rs2 = rs2; }
        public override void Execute(MachineState s, InstructionData d) {
             if (s.Config.XLEN == 32)
            {
                long v1 = (int)s.Registers.Read(d.Rs1); // Signed
                ulong v2 = (uint)s.Registers.Read(d.Rs2); // Unsigned
                // Cast to long works because v2 (32-bit) fits in long (64-bit) positively
                long res = v1 * (long)v2;
                s.Registers.Write(d.Rd, (ulong)(res >> 32));
            }
            else
            {
                long v1 = (long)s.Registers.Read(d.Rs1);
                ulong v2 = s.Registers.Read(d.Rs2);
                Int128 res = (Int128)v1 * (Int128)v2;
                s.Registers.Write(d.Rd, (ulong)(res >> 64));
            }
        }
    }
}
