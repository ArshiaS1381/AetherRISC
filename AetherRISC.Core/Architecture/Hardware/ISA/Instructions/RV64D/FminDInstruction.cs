using System;
using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
namespace AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64D;

[RiscvInstruction("FMIN.D", InstructionSet.RV64D, RiscvEncodingType.R, 0x53, Funct3 = 0, Funct7 = 0x15,
    Name = "Minimum (Double)", 
    Description = "Stores the smaller double-precision value in rd.", 
    Usage = "fmin.d fd, fs1, fs2")]
public class FminDInstruction : RTypeInstruction
{
    public FminDInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }
    public override void Execute(MachineState s, InstructionData d) =>
        s.FRegisters.WriteDouble(d.Rd, Math.Min(s.FRegisters.ReadDouble(d.Rs1), s.FRegisters.ReadDouble(d.Rs2)));
}


