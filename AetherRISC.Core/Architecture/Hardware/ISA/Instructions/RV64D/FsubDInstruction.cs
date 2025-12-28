using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
namespace AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64D;

[RiscvInstruction("FSUB.D", InstructionSet.RV64D, RiscvEncodingType.R, 0x53, Funct3 = 7, Funct7 = 0x05)]
public class FsubDInstruction : RTypeInstruction
{
    public FsubDInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }
    public override void Execute(MachineState s, InstructionData d) => s.FRegisters.WriteDouble(d.Rd, s.FRegisters.ReadDouble(d.Rs1) - s.FRegisters.ReadDouble(d.Rs2));
}
