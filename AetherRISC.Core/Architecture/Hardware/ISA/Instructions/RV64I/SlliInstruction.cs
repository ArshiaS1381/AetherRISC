using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
namespace AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64I;

[RiscvInstruction("SLLI", InstructionSet.RV64I, RiscvEncodingType.ShiftImm, 0x13, Funct3 = 1, Funct6 = 0x00,
    Name = "Shift Left Logical Immediate",
    Description = "Shifts the value in rs1 left by a constant shift amount (shamt), storing the result in rd.",
    Usage = "slli rd, rs1, shamt")]
public class SlliInstruction : ITypeInstruction
{
    public SlliInstruction(int rd, int rs1, int imm) : base(rd, rs1, imm) { }

    public override void Execute(MachineState s, InstructionData d)
    {
        s.Registers.Write(d.Rd, s.Registers.Read(d.Rs1) << (int)(d.Immediate & 0x3F));
    }
}
