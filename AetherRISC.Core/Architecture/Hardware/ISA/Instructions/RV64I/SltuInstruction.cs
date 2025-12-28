using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
namespace AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64I;

[RiscvInstruction("SLTU", InstructionSet.RV64I, RiscvEncodingType.R, 0x33, Funct3 = 3, Funct7 = 0,
    Name = "Set Less Than Unsigned",
    Description = "Performs an unsigned comparison. If rs1 < rs2, stores 1 in rd; otherwise stores 0.",
    Usage = "sltu rd, rs1, rs2")]
public class SltuInstruction : RTypeInstruction
{
    public SltuInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }

    public override void Execute(MachineState s, InstructionData d)
    {
        s.Registers.Write(d.Rd, s.Registers.Read(d.Rs1) < s.Registers.Read(d.Rs2) ? 1ul : 0ul);
    }
}
