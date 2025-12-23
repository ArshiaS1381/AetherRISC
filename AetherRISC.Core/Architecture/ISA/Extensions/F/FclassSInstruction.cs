using AetherRISC.Core.Architecture;
using AetherRISC.Core.Architecture.ISA.Base;
using System;

namespace AetherRISC.Core.Architecture.ISA.Extensions.F
{
    public class FclassSInstruction : Instruction {
        public override string Mnemonic => "FCLASS.S";
        public override int Rd { get; } public override int Rs1 { get; }
        public FclassSInstruction(int rd, int rs1) { Rd = rd; Rs1 = rs1; }
        public override void Execute(MachineState s, InstructionData d) {
            float val = s.FRegisters.ReadSingle(d.Rs1);
            uint bits = BitConverter.SingleToUInt32Bits(val);
            uint result = 0;

            bool isNeg = (bits >> 31) != 0;
            uint exponent = (bits >> 23) & 0xFF;
            uint fraction = bits & 0x7FFFFF;

            if (exponent == 0xFF) {
                if (fraction == 0) result = isNeg ? 1u << 0 : 1u << 7; // Infinity
                else {
                    // NaN: Bit 22 determines Signaling (0) vs Quiet (1)
                    bool isQuiet = (fraction & 0x400000) != 0;
                    result = isQuiet ? 1u << 9 : 1u << 8;
                }
            } else if (exponent == 0) {
                if (fraction == 0) result = isNeg ? 1u << 3 : 1u << 4; // Zero
                else result = isNeg ? 1u << 2 : 1u << 5; // Subnormal
            } else {
                result = isNeg ? 1u << 1 : 1u << 6; // Normal
            }

            s.Registers.Write(d.Rd, result);
        }
    }
}
