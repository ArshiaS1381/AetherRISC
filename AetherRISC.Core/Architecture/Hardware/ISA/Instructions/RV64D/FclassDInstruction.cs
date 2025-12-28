using AetherRISC.Core.Architecture.Hardware.ISA;
using System;
using AetherRISC.Core.Architecture.Simulation.State;
namespace AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64D;

[RiscvInstruction("FCLASS.D", InstructionSet.RV64D, RiscvEncodingType.R, 0x53, Funct3 = 1, Funct7 = 0x71,
    Name = "Floating-Point Classify (Double)", 
    Description = "Writes a 10-bit mask to integer register rd identifying the type of the double-precision value in rs1.", 
    Usage = "fclass.d rd, fs1")]
public class FclassDInstruction : RTypeInstruction
{
    public FclassDInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }

    public override void Execute(MachineState s, InstructionData d)
    {
        double val = s.FRegisters.ReadDouble(d.Rs1);
        ulong bits = BitConverter.DoubleToUInt64Bits(val);
        
        bool isNeg = (bits >> 63) != 0;
        int exponent = (int)((bits >> 52) & 0x7FF);
        ulong fraction = bits & 0xFFFFFFFFFFFFF;

        uint mask = 0;
        if (exponent == 0x7FF) {
            if (fraction == 0) mask = isNeg ? 1u << 0 : 1u << 7; // -Inf / +Inf
            else mask = ((fraction & 0x8000000000000) != 0) ? 1u << 9 : 1u << 8; // sNaN / qNaN
        } else if (exponent == 0) {
            if (fraction == 0) mask = isNeg ? 1u << 3 : 1u << 4; // -0 / +0
            else mask = isNeg ? 1u << 2 : 1u << 5; // -Subnormal / +Subnormal
        } else {
            mask = isNeg ? 1u << 1 : 1u << 6; // -Normal / +Normal
        }
        s.Registers.Write(d.Rd, mask);
    }
}
