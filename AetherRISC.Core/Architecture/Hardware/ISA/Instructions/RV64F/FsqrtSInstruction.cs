using AetherRISC.Core.Architecture.Hardware.ISA;
using System;
using AetherRISC.Core.Architecture.Simulation.State;
namespace AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64F;

[RiscvInstruction("FSQRT.S", InstructionSet.RV64F, RiscvEncodingType.R, 0x53, Funct3 = 7, Funct7 = 0x2C,
    Name = "Floating-Point Square Root (Single)", 
    Description = "Calculates the square root of the single-precision value in rs1.", 
    Usage = "fsqrt.s fd, fs1")]
public class FsqrtSInstruction : RTypeInstruction
{
    public FsqrtSInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }

    public override void Execute(MachineState s, InstructionData d)
    {
        float v1 = s.FRegisters.ReadSingle(d.Rs1);
        s.FRegisters.WriteSingle(d.Rd, (float)Math.Sqrt(v1));
    }
}
