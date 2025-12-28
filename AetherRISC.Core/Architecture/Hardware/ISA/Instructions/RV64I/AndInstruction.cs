using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
namespace AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64I;

[RiscvInstruction("AND", InstructionSet.RV64I, RiscvEncodingType.R, 0x33, Funct3 = 7, Funct7 = 0,
    Name = "Logical AND",
    Description = "Performs a bitwise AND operation between registers rs1 and rs2, storing the result in rd.",
    Usage = "and rd, rs1, rs2")]
public class AndInstruction : RTypeInstruction
{
    public AndInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }

    public override void Execute(MachineState s, InstructionData d)
    {
        s.Registers.Write(d.Rd, s.Registers.Read(d.Rs1) & s.Registers.Read(d.Rs2));
    }
}
