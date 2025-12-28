using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
namespace AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64I;

[RiscvInstruction("BEQ", InstructionSet.RV64I, RiscvEncodingType.B, 0x63, Funct3 = 0,
    Name = "Branch Equal",
    Description = "Branches to the offset if rs1 equals rs2.",
    Usage = "beq rs1, rs2, offset")]
public class BeqInstruction : BTypeInstruction
{
    public BeqInstruction(int rs1, int rs2, int imm) : base(rs1, rs2, imm) { }

    public override void Execute(MachineState s, InstructionData d)
    {
        if (s.Registers.Read(d.Rs1) == s.Registers.Read(d.Rs2))
        {
            s.ProgramCounter = d.PC + (ulong)(long)(int)d.Immediate;
        }
    }
}
