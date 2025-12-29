using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.ISA; 
using AetherRISC.Core.Architecture.Hardware.Pipeline;

namespace AetherRISC.Core.Abstractions.Interfaces;

public interface IInstruction
{
    string Mnemonic { get; }
    bool IsLoad { get; }
    bool IsStore { get; }
    bool IsBranch { get; }
    bool IsJump { get; }

    int Rd { get; }
    int Rs1 { get; }
    int Rs2 { get; }
    int Imm { get; }

    // Legacy execution for SimpleRunner
    void Execute(MachineState state, InstructionData data);

    /// <summary>
    /// Calculates the result of the instruction for the Pipeline Execute stage.
    /// Default implementation in InstructionBase contains the legacy string-matching fallback.
    /// Override this in specific instructions for performance.
    /// </summary>
    void Compute(MachineState state, ulong rs1Val, ulong rs2Val, PipelineBuffers buffers);
}
