using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.Pipeline;

namespace AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RvSystem;

[RiscvInstruction("ECALL", InstructionSet.RV64I, RiscvEncodingType.I, 0x73, Funct3 = 0,
    Name = "Environment Call",
    Description = "Raises an Environment Call exception.",
    Usage = "ecall")]
public class EcallInstruction : ITypeInstruction
{
    // FIX: Force imm to 0 strictly
    public EcallInstruction(int rd, int rs1, int imm) : base(rd, rs1, 0) { }

    public override void Execute(MachineState s, InstructionData d)
    {
        if (s.Host != null) s.Host.HandleEcall(s);
    }

    public override void Compute(MachineState state, ulong rs1Val, ulong rs2Val, PipelineBuffers buffers)
    {
        // ECALL doesn't perform an ALU operation.
        // The System Call is handled in the Writeback (Commit) stage.
        buffers.ExecuteMemory.AluResult = 0;
    }
}
