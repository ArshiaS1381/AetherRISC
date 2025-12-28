using System;
using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
namespace AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64D;

[RiscvInstruction("FMAX.D", InstructionSet.RV64D, RiscvEncodingType.R, 0x53, Funct3 = 1, Funct7 = 0x15,
    Name = "Maximum (Double)", 
    Description = "Stores the larger double-precision value in rd.", 
    Usage = "fmax.d fd, fs1, fs2")]
public class FmaxDInstruction : RTypeInstruction
{
    public FmaxDInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }
    public override void Execute(MachineState s, InstructionData d) =>
        s.FRegisters.WriteDouble(d.Rd, Math.Max(s.FRegisters.ReadDouble(d.Rs1), s.FRegisters.ReadDouble(d.Rs2)));
}


