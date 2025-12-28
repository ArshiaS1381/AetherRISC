using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
namespace AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64I;

[RiscvInstruction("SLT", InstructionSet.RV64I, RiscvEncodingType.R, 0x33, Funct3 = 2, Funct7 = 0,
    Name = "Set Less Than",
    Description = "Performs a signed comparison. If rs1 < rs2, stores 1 in rd; otherwise stores 0.",
    Usage = "slt rd, rs1, rs2")]
public class SltInstruction : RTypeInstruction
{
    public SltInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }

    public override void Execute(MachineState s, InstructionData d)
    {
        s.Registers.Write(d.Rd, (long)s.Registers.Read(d.Rs1) < (long)s.Registers.Read(d.Rs2) ? 1ul : 0ul);
    }
}
