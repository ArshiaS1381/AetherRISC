using System;
using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
namespace AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64D;

[RiscvInstruction("FSQRT.D", InstructionSet.RV64D, RiscvEncodingType.R, 0x53, Funct3 = 7, Funct7 = 0x2D)]
public class FsqrtDInstruction : RTypeInstruction
{
    public FsqrtDInstruction(int rd, int rs1, int rs2 = 0) : base(rd, rs1, rs2) { }
    public override void Execute(MachineState s, InstructionData d) => s.FRegisters.WriteDouble(d.Rd, Math.Sqrt(s.FRegisters.ReadDouble(d.Rs1)));
}
