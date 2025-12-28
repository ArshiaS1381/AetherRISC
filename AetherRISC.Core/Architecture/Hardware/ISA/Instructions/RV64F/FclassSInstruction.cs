using AetherRISC.Core.Architecture.Hardware.ISA;
using System;
using AetherRISC.Core.Architecture.Simulation.State;
namespace AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64F;

[RiscvInstruction("FCLASS.S", InstructionSet.RV64F, RiscvEncodingType.R, 0x53, Funct3 = 1, Funct7 = 0x70,
    Name = "Floating-Point Classify (Single)", 
    Description = "Examines the single-precision value in rs1 and writes a 10-bit mask to integer register rd identifying the value's type (NaN, Infinity, Zero, etc.).", 
    Usage = "fclass.s rd, fs1")]
public class FclassSInstruction : RTypeInstruction
{
    public FclassSInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }

    public override void Execute(MachineState s, InstructionData d)
    {
        float val = s.FRegisters.ReadSingle(d.Rs1);
        uint bits = BitConverter.SingleToUInt32Bits(val);
        bool isNeg = (bits >> 31) != 0;
        uint exponent = (bits >> 23) & 0xFF;
        uint fraction = bits & 0x7FFFFF;

        uint mask = 0;
        if (exponent == 0xFF) {
            if (fraction == 0) mask = isNeg ? 1u << 0 : 1u << 7; // -Inf / +Inf
            else mask = ((fraction & 0x400000) != 0) ? 1u << 9 : 1u << 8; // sNaN / qNaN
        } else if (exponent == 0) {
            if (fraction == 0) mask = isNeg ? 1u << 3 : 1u << 4; // -0 / +0
            else mask = isNeg ? 1u << 2 : 1u << 5; // -Subnormal / +Subnormal
        } else {
            mask = isNeg ? 1u << 1 : 1u << 6; // -Normal / +Normal
        }
        s.Registers.Write(d.Rd, mask);
    }
}
