using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.Pipeline;

namespace AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64I;

[RiscvInstruction("FENCE", InstructionSet.RV64I, RiscvEncodingType.I, 0x0F, Funct3 = 0,
    Name = "Fence",
    Description = "Orders memory access. In this simulation, it acts as a No-Op as memory is strongly ordered.",
    Usage = "fence pred, succ")]
public class FenceInstruction : ITypeInstruction
{
    public FenceInstruction(int rd, int rs1, int imm) : base(rd, rs1, imm) { }

    public override void Execute(MachineState s, InstructionData d) { }

    public override void Compute(MachineState state, ulong rs1Val, ulong rs2Val, PipelineBuffers buffers)
    {
        // No-Op in simulation
        buffers.ExecuteMemory.AluResult = 0;
    }
}
