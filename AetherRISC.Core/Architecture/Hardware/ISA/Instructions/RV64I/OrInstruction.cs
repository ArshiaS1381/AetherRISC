using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
namespace AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64I;

[RiscvInstruction("OR", InstructionSet.RV64I, RiscvEncodingType.R, 0x33, Funct3 = 6, Funct7 = 0,
    Name = "Or", 
    Description = "Performs a bitwise logical OR between rs1 and rs2, storing the result in rd.", 
    Usage = "or rd, rs1, rs2")]
public class OrInstruction : RTypeInstruction 
{
    public OrInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }

    public override void Execute(MachineState s, InstructionData d) 
    {
        s.Registers.Write(d.Rd, s.Registers.Read(d.Rs1) | s.Registers.Read(d.Rs2));
    }
}
