using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
namespace AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64F;

[RiscvInstruction("FEQ.S", InstructionSet.RV64F, RiscvEncodingType.R, 0x53, Funct3 = 2, Funct7 = 0x50,
    Name = "Floating-Point Equal (Single)", 
    Description = "Signed comparison for equality between single-precision values. Stores 1 in integer register rd if rs1 == rs2.", 
    Usage = "feq.s rd, fs1, fs2")]
public class FeqSInstruction : RTypeInstruction
{
    public FeqSInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }

    public override void Execute(MachineState s, InstructionData d)
    {
        float v1 = s.FRegisters.ReadSingle(d.Rs1);
        float v2 = s.FRegisters.ReadSingle(d.Rs2);
        s.Registers.Write(d.Rd, (v1 == v2) ? 1u : 0u);
    }
}
