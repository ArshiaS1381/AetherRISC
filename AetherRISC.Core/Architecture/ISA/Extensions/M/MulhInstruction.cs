using AetherRISC.Core.Architecture;
using AetherRISC.Core.Architecture.ISA.Base;
using System;

namespace AetherRISC.Core.Architecture.ISA.Extensions.M
{
    public class MulhInstruction : Instruction {
        public override string Mnemonic => "MULH";
        public override int Rd { get; } public override int Rs1 { get; } public override int Rs2 { get; }
        public MulhInstruction(int rd, int rs1, int rs2) { Rd = rd; Rs1 = rs1; Rs2 = rs2; }
        public override void Execute(MachineState s, InstructionData d) {
            if (s.Config.XLEN == 32)
            {
                // RV32: 32x32 -> 64-bit result. High part is upper 32 bits.
                long v1 = (int)s.Registers.Read(d.Rs1);
                long v2 = (int)s.Registers.Read(d.Rs2);
                long res = v1 * v2;
                s.Registers.Write(d.Rd, (ulong)(res >> 32)); // Sign-extension handled by write logic if any
            }
            else
            {
                // RV64: 64x64 -> 128-bit result.
                long v1 = (long)s.Registers.Read(d.Rs1);
                long v2 = (long)s.Registers.Read(d.Rs2);
                Int128 res = (Int128)v1 * (Int128)v2;
                s.Registers.Write(d.Rd, (ulong)(res >> 64));
            }
        }
    }
}
