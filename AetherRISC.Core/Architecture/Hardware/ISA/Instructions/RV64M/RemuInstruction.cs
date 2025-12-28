using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
namespace AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64M;

[RiscvInstruction("REMU", InstructionSet.RV64M, RiscvEncodingType.R, 0x33, Funct3 = 7, Funct7 = 1,
    Name = "Remainder Unsigned", 
    Description = "Unsigned remainder of the division of rs1 by rs2.", 
    Usage = "remu rd, rs1, rs2")]
public class RemuInstruction : RTypeInstruction
{
    public RemuInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }

    public override void Execute(MachineState s, InstructionData d)
    {
        ulong v1 = s.Registers.Read(d.Rs1);
        ulong v2 = s.Registers.Read(d.Rs2);
        if (v2 == 0) s.Registers.Write(d.Rd, v1);
        else s.Registers.Write(d.Rd, v1 % v2);
    }
}
