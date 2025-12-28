using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
namespace AetherRISC.Core.Architecture.Hardware.ISA.Extensions.B.Zbb;

[RiscvInstruction("MINU", InstructionSet.Zbb, RiscvEncodingType.R, 0x33, Funct3 = 5, Funct7 = 0x05,
    Name = "Minimum Unsigned", 
    Description = "Compares unsigned integers in rs1 and rs2, storing the smaller value in rd.", 
    Usage = "minu rd, rs1, rs2")]
public class MinuInstruction : RTypeInstruction
{
    public MinuInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }

    public override void Execute(MachineState s, InstructionData d) =>
        s.Registers.Write(d.Rd, Math.Min(s.Registers.Read(d.Rs1), s.Registers.Read(d.Rs2)));
}
