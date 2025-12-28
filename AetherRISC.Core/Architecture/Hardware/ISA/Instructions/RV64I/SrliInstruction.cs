using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
namespace AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64I;

[RiscvInstruction("SRLI", InstructionSet.RV64I, RiscvEncodingType.ShiftImm, 0x13, Funct3 = 5, Funct6 = 0x00,
    Name = "Shift Right Logical Immediate",
    Description = "Shifts register rs1 right by a constant shift amount (shamt). Vacated bits are zero-filled.",
    Usage = "srli rd, rs1, shamt")]
public class SrliInstruction : ITypeInstruction
{
    public SrliInstruction(int rd, int rs1, int imm) : base(rd, rs1, imm) { }

    public override void Execute(MachineState s, InstructionData d)
    {
        // Shift amount is encoded in the lower bits of the immediate field.
        s.Registers.Write(d.Rd, s.Registers.Read(d.Rs1) >> (int)(d.Immediate & 0x3F));
    }
}
