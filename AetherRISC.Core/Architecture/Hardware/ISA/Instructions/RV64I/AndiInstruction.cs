using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
namespace AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64I;

[RiscvInstruction("ANDI", InstructionSet.RV64I, RiscvEncodingType.I, 0x13, Funct3 = 7,
    Name = "And Immediate",
    Description = "Performs a bitwise AND between register rs1 and the sign-extended 12-bit immediate, storing the result in rd.",
    Usage = "andi rd, rs1, imm")]
public class AndiInstruction : ITypeInstruction
{
    public AndiInstruction(int rd, int rs1, int imm) : base(rd, rs1, imm) { }

    public override void Execute(MachineState s, InstructionData d)
    {
        // ANDI sign-extends the 12-bit immediate to XLEN before the AND operation.
        s.Registers.Write(d.Rd, s.Registers.Read(d.Rs1) & (ulong)(long)d.Immediate);
    }
}
