using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
namespace AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64M;

[RiscvInstruction("DIVUW", InstructionSet.RV64M, RiscvEncodingType.R, 0x3B, Funct3 = 5, Funct7 = 1,
    Name = "Divide Word Unsigned", 
    Description = "Divides the lower 32 bits of rs1 by rs2 (unsigned), sign-extending the 32-bit quotient to 64 bits.", 
    Usage = "divuw rd, rs1, rs2")]
public class DivuwInstruction : RTypeInstruction
{
    public DivuwInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }

    public override void Execute(MachineState s, InstructionData d)
    {
        uint v1 = (uint)s.Registers.Read(d.Rs1);
        uint v2 = (uint)s.Registers.Read(d.Rs2);

        if (v2 == 0)
        {
            s.Registers.Write(d.Rd, ulong.MaxValue);
        }
        else
        {
            s.Registers.Write(d.Rd, (ulong)(long)(int)(v1 / v2));
        }
    }
}


