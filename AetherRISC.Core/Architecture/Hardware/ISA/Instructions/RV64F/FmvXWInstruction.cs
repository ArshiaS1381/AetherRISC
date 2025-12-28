using AetherRISC.Core.Architecture.Hardware.ISA;
using System;
using AetherRISC.Core.Architecture.Simulation.State;
namespace AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64F;

[RiscvInstruction("FMV.X.W", InstructionSet.RV64F, RiscvEncodingType.R, 0x53, Funct3 = 0, Funct7 = 0x70,
    Name = "Move from Float to Integer (Single)", 
    Description = "Moves the bit pattern from floating-point register rs1 to integer register rd. Result is sign-extended.", 
    Usage = "fmv.x.w rd, fs1")]
public class FmvXWInstruction : RTypeInstruction
{
    public FmvXWInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }

    public override void Execute(MachineState s, InstructionData d)
    {
        float fval = s.FRegisters.ReadSingle(d.Rs1);
        int bits = BitConverter.SingleToInt32Bits(fval);
        s.Registers.Write(d.Rd, (ulong)(long)bits);
    }
}
