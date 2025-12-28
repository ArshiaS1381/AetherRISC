using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
namespace AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64I;

[RiscvInstruction("WFI", InstructionSet.RV64I, RiscvEncodingType.R, 0x73, Funct3 = 0, Funct7 = 0x08,
    Name = "Wait For Interrupt",
    Description = "Suspends execution until an interrupt is received. Currently behaves as a NOP.",
    Usage = "wfi")]
public class WfiInstruction : RTypeInstruction 
{
    // WFI is encoded as R-type with rs2 = 5
    public override int Rs2 => 5;

    public WfiInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }

    public override void Execute(MachineState s, InstructionData d) { /* Sleep logic placeholder */ }
}
