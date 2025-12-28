using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
namespace AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64I;

[RiscvInstruction("SLTI", InstructionSet.RV64I, RiscvEncodingType.I, 0x13, Funct3 = 2,
    Name = "Set Less Than Immediate",
    Description = "Signed comparison between rs1 and the sign-extended 12-bit immediate. If rs1 < imm, stores 1 in rd; otherwise stores 0.",
    Usage = "slti rd, rs1, imm")]
public class SltiInstruction : ITypeInstruction
{
    public SltiInstruction(int rd, int rs1, int imm) : base(rd, rs1, imm) { }

    public override void Execute(MachineState s, InstructionData d)
    {
        s.Registers.Write(d.Rd, (long)s.Registers.Read(d.Rs1) < (long)d.Immediate ? 1ul : 0ul);
    }
}
