using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
namespace AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64M;

[RiscvInstruction("DIVU", InstructionSet.RV64M, RiscvEncodingType.R, 0x33, Funct3 = 5, Funct7 = 1,
    Name = "Divide Unsigned", 
    Description = "Performs an unsigned integer division of rs1 by rs2, storing the quotient in rd.", 
    Usage = "divu rd, rs1, rs2")]
public class DivuInstruction : RTypeInstruction
{
    public DivuInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }

    public override void Execute(MachineState s, InstructionData d)
    {
        ulong v1 = s.Registers.Read(d.Rs1);
        ulong v2 = s.Registers.Read(d.Rs2);

        if (v2 == 0)
        {
            s.Registers.Write(d.Rd, ulong.MaxValue);
        }
        else
        {
            ulong res = v1 / v2;
            if (s.Config.XLEN == 32) res = (ulong)(uint)res;
            s.Registers.Write(d.Rd, res);
        }
    }
}


