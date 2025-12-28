using System;
using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
namespace AetherRISC.Core.Architecture.Hardware.ISA.Extensions.B.Zbb;

[RiscvInstruction("MAXU", InstructionSet.Zbb, RiscvEncodingType.R, 0x33, Funct3 = 7, Funct7 = 0x05,
    Name = "Maximum Unsigned",
    Description = "Stores the larger unsigned integer in rd.",
    Usage = "maxu rd, rs1, rs2")]
public class MaxuInstruction : RTypeInstruction {
    public MaxuInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }
    public override void Execute(MachineState s, InstructionData d) =>
        s.Registers.Write(d.Rd, System.Math.Max(s.Registers.Read(d.Rs1), s.Registers.Read(d.Rs2)));
}
