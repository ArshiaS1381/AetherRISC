using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
namespace AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64M;

[RiscvInstruction("REM", InstructionSet.RV64M, RiscvEncodingType.R, 0x33, Funct3 = 6, Funct7 = 1,
    Name = "Remainder", 
    Description = "Signed remainder of the division of rs1 by rs2.", 
    Usage = "rem rd, rs1, rs2")]
public class RemInstruction : RTypeInstruction
{
    public RemInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }

    public override void Execute(MachineState s, InstructionData d)
    {
        long v1 = (long)s.Registers.Read(d.Rs1);
        long v2 = (long)s.Registers.Read(d.Rs2);
        if (v2 == 0) s.Registers.Write(d.Rd, (ulong)v1);
        else if (v1 == long.MinValue && v2 == -1) s.Registers.Write(d.Rd, 0);
        else s.Registers.Write(d.Rd, (ulong)(v1 % v2));
    }
}
