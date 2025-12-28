using AetherRISC.Core.Architecture.Hardware.ISA;
using System;
using AetherRISC.Core.Architecture.Simulation.State;
namespace AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64F;

[RiscvInstruction("FMV.W.X", InstructionSet.RV64F, RiscvEncodingType.R, 0x53, Funct3 = 0, Funct7 = 0x78,
    Name = "Move from Integer to Float (Single)", 
    Description = "Moves the bit pattern from integer register rs1 to floating-point register rd without conversion.", 
    Usage = "fmv.w.x fd, rs1")]
public class FmvWXInstruction : RTypeInstruction
{
    public FmvWXInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }

    public override void Execute(MachineState s, InstructionData d)
    {
        uint bits = (uint)s.Registers.Read(d.Rs1);
        s.FRegisters.WriteSingle(d.Rd, BitConverter.Int32BitsToSingle((int)bits));
    }
}
