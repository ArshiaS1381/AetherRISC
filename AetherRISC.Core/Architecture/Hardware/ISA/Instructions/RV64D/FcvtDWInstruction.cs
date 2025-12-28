using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
namespace AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64D;

[RiscvInstruction("FCVT.D.W", InstructionSet.RV64D, RiscvEncodingType.R, 0x53, Funct3 = 7, Funct7 = 0x69,
    Name = "Convert Word to Double", 
    Description = "Converts a 32-bit signed integer in rs1 to a double-precision floating-point value in rd.", 
    Usage = "fcvt.d.w fd, rs1")]
public class FcvtDWInstruction : RTypeInstruction
{
    public FcvtDWInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }

    public override void Execute(MachineState s, InstructionData d)
    {
        int v1 = (int)s.Registers.Read(d.Rs1);
        s.FRegisters.WriteDouble(d.Rd, (double)v1);
    }
}
