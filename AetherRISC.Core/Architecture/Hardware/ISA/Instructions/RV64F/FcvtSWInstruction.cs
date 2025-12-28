using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
namespace AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64F;

[RiscvInstruction("FCVT.S.W", InstructionSet.RV64F, RiscvEncodingType.R, 0x53, Funct3 = 7, Funct7 = 0x68,
    Name = "Convert Word to Float (Single)", 
    Description = "Converts the signed 32-bit integer in rs1 to a single-precision value, stored in floating-point register fd.", 
    Usage = "fcvt.s.w fd, rs1")]
public class FcvtSWInstruction : RTypeInstruction
{
    public FcvtSWInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }

    public override void Execute(MachineState s, InstructionData d)
    {
        int ival = (int)s.Registers.Read(d.Rs1);
        s.FRegisters.WriteSingle(d.Rd, (float)ival);
    }
}
