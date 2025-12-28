using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
namespace AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64I;

[RiscvInstruction("BNE", InstructionSet.RV64I, RiscvEncodingType.B, 0x63, Funct3 = 1,
    Name = "Branch Not Equal",
    Description = "Branches to the offset if the values in registers rs1 and rs2 are not equal.",
    Usage = "bne rs1, rs2, offset")]
public class BneInstruction : BTypeInstruction
{
    public BneInstruction(int rs1, int rs2, int imm) : base(rs1, rs2, imm) { }
    
    // Compatibility constructor
    public BneInstruction(int rs1, int rs2, int imm, uint dummy) : base(rs1, rs2, imm) { }

    public override void Execute(MachineState s, InstructionData d)
    {
        if (s.Registers.Read(d.Rs1) != s.Registers.Read(d.Rs2))
        {
            s.ProgramCounter = d.PC + (ulong)(long)(int)d.Immediate;
        }
    }
}
