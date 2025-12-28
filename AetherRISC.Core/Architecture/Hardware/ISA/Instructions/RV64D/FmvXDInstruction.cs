using System;
using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
namespace AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64D;

[RiscvInstruction("FMV.X.D", InstructionSet.RV64D, RiscvEncodingType.R, 0x53, Funct3 = 0, Funct7 = 0x71,
    Name = "Move Float to Integer (Double)", 
    Description = "Moves the 64-bit pattern from floating-point register rs1 to integer register rd.", 
    Usage = "fmv.x.d rd, fs1")]
public class FmvXDInstruction : RTypeInstruction
{
    public FmvXDInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }
    public override void Execute(MachineState s, InstructionData d) => 
        s.Registers.Write(d.Rd, (ulong)BitConverter.DoubleToInt64Bits(s.FRegisters.ReadDouble(d.Rs1)));
}


