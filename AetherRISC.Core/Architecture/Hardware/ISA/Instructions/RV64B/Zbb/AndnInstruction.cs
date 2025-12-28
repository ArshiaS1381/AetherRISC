using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
namespace AetherRISC.Core.Architecture.Hardware.ISA.Extensions.B.Zbb;

[RiscvInstruction("ANDN", InstructionSet.Zbb, RiscvEncodingType.R, 0x33, Funct3 = 7, Funct7 = 0x20,
    Name = "AND with Complement", 
    Description = "Performs bitwise AND between rs1 and the bitwise inversion of rs2.", 
    Usage = "andn rd, rs1, rs2")]
public class AndnInstruction : RTypeInstruction
{
    public AndnInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }

    public override void Execute(MachineState s, InstructionData d)
    {
        ulong res = s.Registers.Read(d.Rs1) & ~s.Registers.Read(d.Rs2);
        if (s.Config.XLEN == 32) res = (ulong)(uint)res;
        s.Registers.Write(d.Rd, res);
    }
}
