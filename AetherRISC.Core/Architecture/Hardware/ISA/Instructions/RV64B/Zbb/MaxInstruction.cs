using System;
using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
namespace AetherRISC.Core.Architecture.Hardware.ISA.Extensions.B.Zbb;

[RiscvInstruction("MAX", InstructionSet.Zbb, RiscvEncodingType.R, 0x33, Funct3 = 6, Funct7 = 0x05,
    Name = "Maximum",
    Description = "Stores the larger signed integer in rd.",
    Usage = "max rd, rs1, rs2")]
public class MaxInstruction : RTypeInstruction {
    public MaxInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }
    public override void Execute(MachineState s, InstructionData d) =>
        s.Registers.Write(d.Rd, (ulong)System.Math.Max((long)s.Registers.Read(d.Rs1), (long)s.Registers.Read(d.Rs2)));
}
