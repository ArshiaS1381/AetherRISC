using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
namespace AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64D;

[RiscvInstruction("FCVT.S.D", InstructionSet.RV64D, RiscvEncodingType.R, 0x53, Funct3 = 7, Funct7 = 0x20,
    Name = "Convert Double to Single", 
    Description = "Converts a double-precision value in rs1 to single-precision in rd.", 
    Usage = "fcvt.s.d fd, fs1")]
public class FcvtSDInstruction : RTypeInstruction
{
    public FcvtSDInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }

    public override void Execute(MachineState s, InstructionData d)
    {
        double v1 = s.FRegisters.ReadDouble(d.Rs1);
        s.FRegisters.WriteSingle(d.Rd, (float)v1);
    }
}
