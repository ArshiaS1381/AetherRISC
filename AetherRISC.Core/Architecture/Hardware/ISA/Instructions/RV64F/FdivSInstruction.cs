using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
namespace AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64F;

[RiscvInstruction("FDIV.S", InstructionSet.RV64F, RiscvEncodingType.R, 0x53, Funct3 = 7, Funct7 = 0x0C,
    Name = "Floating-Point Divide (Single)", 
    Description = "Divides the single-precision floating-point value in rs1 by rs2.", 
    Usage = "fdiv.s fd, fs1, fs2")]
public class FdivSInstruction : RTypeInstruction
{
    public FdivSInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }

    public override void Execute(MachineState s, InstructionData d)
    {
        float v1 = s.FRegisters.ReadSingle(d.Rs1);
        float v2 = s.FRegisters.ReadSingle(d.Rs2);
        s.FRegisters.WriteSingle(d.Rd, v1 / v2);
    }
}
