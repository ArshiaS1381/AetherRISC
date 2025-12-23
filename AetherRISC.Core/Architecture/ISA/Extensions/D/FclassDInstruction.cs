using AetherRISC.Core.Architecture;
using AetherRISC.Core.Architecture.ISA.Base;
using System;

namespace AetherRISC.Core.Architecture.ISA.Extensions.D
{
    public class FclassDInstruction : Instruction {
        public override string Mnemonic => "FCLASS.D";
        public override int Rd { get; } public override int Rs1 { get; }
        public FclassDInstruction(int rd, int rs1) { Rd = rd; Rs1 = rs1; }
        public override void Execute(MachineState s, InstructionData d) {
            double val = s.FRegisters.ReadDouble(d.Rs1);
            ulong bits = BitConverter.DoubleToUInt64Bits(val);
            uint result = 0;

            bool isNeg = (bits >> 63) != 0;
            uint exponent = (uint)((bits >> 52) & 0x7FF);
            ulong fraction = bits & 0xFFFFFFFFFFFFFL; // Correct C# UL suffix

            if (exponent == 0x7FF) {
                if (fraction == 0) {
                    result = isNeg ? 1u << 0 : 1u << 7; // Infinity
                } else {
                    // NaN: Bit 51 determines Signaling (0) vs Quiet (1)
                    bool isQuiet = (fraction & 0x8000000000000L) != 0;
                    result = isQuiet ? 1u << 9 : 1u << 8;
                }
            } else if (exponent == 0) {
                if (fraction == 0) {
                    result = isNeg ? 1u << 3 : 1u << 4; // Zero
                } else {
                    result = isNeg ? 1u << 2 : 1u << 5; // Subnormal
                }
            } else {
                result = isNeg ? 1u << 1 : 1u << 6; // Normal
            }

            s.Registers.Write(d.Rd, result);
        }
    }
}
