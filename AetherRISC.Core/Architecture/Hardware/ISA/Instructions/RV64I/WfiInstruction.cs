using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.Pipeline;

namespace AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64I;

[RiscvInstruction("WFI", InstructionSet.RV64I, RiscvEncodingType.R, 0x73, Funct3 = 0, Funct7 = 0x08,
    Name = "Wait For Interrupt",
    Description = "Suspends execution until an interrupt is received. Currently behaves as a NOP.",
    Usage = "wfi")]
public class WfiInstruction : RTypeInstruction 
{
    public override int Rs2 => 5;

    public WfiInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }

    public override void Execute(MachineState s, InstructionData d) { }

    public override void Compute(MachineState state, ulong rs1Val, ulong rs2Val, PipelineBuffers buffers)
    {
        // No-Op in simulation
        buffers.ExecuteMemory.AluResult = 0;
    }
}
