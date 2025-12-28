using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
namespace AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64I;

[RiscvInstruction("ORI", InstructionSet.RV64I, RiscvEncodingType.I, 0x13, Funct3 = 6,
    Name = "Or Immediate", 
    Description = "Performs a bitwise logical OR between rs1 and the sign-extended 12-bit immediate.", 
    Usage = "ori rd, rs1, imm")]
public class OriInstruction : ITypeInstruction 
{
    public OriInstruction(int rd, int rs1, int imm) : base(rd, rs1, imm) { }

    public override void Execute(MachineState s, InstructionData d) 
    {
        s.Registers.Write(d.Rd, s.Registers.Read(d.Rs1) | (ulong)(long)d.Immediate);
    }
}
