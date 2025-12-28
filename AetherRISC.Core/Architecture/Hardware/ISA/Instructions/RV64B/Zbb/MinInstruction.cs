using System;
using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
namespace AetherRISC.Core.Architecture.Hardware.ISA.Extensions.B.Zbb;

[RiscvInstruction("MIN", InstructionSet.Zbb, RiscvEncodingType.R, 0x33, Funct3 = 4, Funct7 = 0x05,
    Name = "Minimum",
    Description = "Stores the smaller signed integer in rd.",
    Usage = "min rd, rs1, rs2")]
public class MinInstruction : RTypeInstruction {
    public MinInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }
    public override void Execute(MachineState s, InstructionData d) =>
        s.Registers.Write(d.Rd, (ulong)System.Math.Min((long)s.Registers.Read(d.Rs1), (long)s.Registers.Read(d.Rs2)));
}
