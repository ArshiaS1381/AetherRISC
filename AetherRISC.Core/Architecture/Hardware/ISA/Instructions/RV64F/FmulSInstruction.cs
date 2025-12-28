using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
namespace AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64F;

[RiscvInstruction("FMUL.S", InstructionSet.RV64F, RiscvEncodingType.R, 0x53, Funct3 = 7, Funct7 = 0x08,
    Name = "Floating-Point Multiply (Single)", 
    Description = "Multiplies the single-precision floating-point values in rs1 and rs2, storing the result in rd.", 
    Usage = "fmul.s fd, fs1, fs2")]
public class FmulSInstruction : RTypeInstruction
{
    public FmulSInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }

    public override void Execute(MachineState s, InstructionData d)
    {
        float v1 = s.FRegisters.ReadSingle(d.Rs1);
        float v2 = s.FRegisters.ReadSingle(d.Rs2);
        s.FRegisters.WriteSingle(d.Rd, v1 * v2);
    }
}
