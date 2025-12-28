using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
namespace AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64F;

[RiscvInstruction("FLT.S", InstructionSet.RV64F, RiscvEncodingType.R, 0x53, Funct3 = 1, Funct7 = 0x50,
    Name = "Floating-Point Less Than (Single)", 
    Description = "Stores 1 in integer register rd if single-precision rs1 < rs2.", 
    Usage = "flt.s rd, fs1, fs2")]
public class FltSInstruction : RTypeInstruction
{
    public FltSInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }

    public override void Execute(MachineState s, InstructionData d)
    {
        float v1 = s.FRegisters.ReadSingle(d.Rs1);
        float v2 = s.FRegisters.ReadSingle(d.Rs2);
        s.Registers.Write(d.Rd, (v1 < v2) ? 1u : 0u);
    }
}
