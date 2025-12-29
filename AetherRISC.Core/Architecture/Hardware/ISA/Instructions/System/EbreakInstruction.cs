using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.Pipeline;

namespace AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RvSystem;

[RiscvInstruction("EBREAK", InstructionSet.RV64I, RiscvEncodingType.I, 0x73, Funct3 = 0,
    Name = "Environment Break",
    Description = "Returns control to the debugging environment.",
    Usage = "ebreak")]
public class EbreakInstruction : ITypeInstruction
{
    // FIX: Force imm to 1 strictly to differentiate from ECALL (0)
    public EbreakInstruction(int rd, int rs1, int imm) : base(rd, rs1, 1) { }

    public override void Execute(MachineState s, InstructionData d)
    {
        s.Halted = true; 
        if (s.Host != null) s.Host.HandleBreak(s);
    }

    public override void Compute(MachineState state, ulong rs1Val, ulong rs2Val, PipelineBuffers buffers)
    {
        // EBREAK doesn't perform an ALU operation.
        // The actual halt/trap logic is handled in the Writeback (Commit) stage
        // to prevent halting on a speculative instruction that might be flushed.
        buffers.ExecuteMemory.AluResult = 0;
    }
}
