using System;
using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
namespace AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64D;

[RiscvInstruction("FMV.D.X", InstructionSet.RV64D, RiscvEncodingType.R, 0x53, Funct3 = 0, Funct7 = 0x79,
    Name = "Move Integer to Float (Double)", 
    Description = "Moves the 64-bit pattern from integer register rs1 to floating-point register rd.", 
    Usage = "fmv.d.x fd, rs1")]
public class FmvDXInstruction : RTypeInstruction
{
    public FmvDXInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }
    public override void Execute(MachineState s, InstructionData d) => 
        s.FRegisters.WriteDouble(d.Rd, BitConverter.Int64BitsToDouble((long)s.Registers.Read(d.Rs1)));
}


