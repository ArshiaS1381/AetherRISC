using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
namespace AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64F;

[RiscvInstruction("FCVT.W.S", InstructionSet.RV64F, RiscvEncodingType.R, 0x53, Funct3 = 7, Funct7 = 0x60,
    Name = "Convert Float to Word (Single)", 
    Description = "Converts the single-precision value in fs1 to a signed 32-bit integer, stored in integer register rd.", 
    Usage = "fcvt.w.s rd, fs1")]
public class FcvtWSInstruction : RTypeInstruction
{
    public FcvtWSInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }

    public override void Execute(MachineState s, InstructionData d)
    {
        float fval = s.FRegisters.ReadSingle(d.Rs1);
        s.Registers.Write(d.Rd, (ulong)(long)(int)fval);
    }
}
