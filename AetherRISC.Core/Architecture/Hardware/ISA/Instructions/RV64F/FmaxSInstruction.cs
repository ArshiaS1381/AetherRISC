using AetherRISC.Core.Architecture.Hardware.ISA;
using System;
using AetherRISC.Core.Architecture.Simulation.State;
namespace AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64F;

[RiscvInstruction("FMAX.S", InstructionSet.RV64F, RiscvEncodingType.R, 0x53, Funct3 = 1, Funct7 = 0x14,
    Name = "Floating-Point Maximum (Single)", 
    Description = "Compares single-precision values in rs1 and rs2, storing the larger value in rd.", 
    Usage = "fmax.s fd, fs1, fs2")]
public class FmaxSInstruction : RTypeInstruction
{
    public FmaxSInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }

    public override void Execute(MachineState s, InstructionData d)
    {
        float v1 = s.FRegisters.ReadSingle(d.Rs1);
        float v2 = s.FRegisters.ReadSingle(d.Rs2);
        s.FRegisters.WriteSingle(d.Rd, Math.Max(v1, v2));
    }
}
