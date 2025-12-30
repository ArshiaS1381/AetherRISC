using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.Pipeline;

namespace AetherRISC.Core.Abstractions.Interfaces
{
    public interface IInstruction
    {
        string Mnemonic { get; }
        int Rd { get; }
        int Rs1 { get; }
        int Rs2 { get; }
        int Imm { get; }
        bool IsLoad { get; }
        bool IsStore { get; }
        bool IsBranch { get; }
        bool IsJump { get; }
        bool IsFloatRegWrite { get; }

        void Execute(MachineState state, InstructionData data);
        void Compute(MachineState state, ulong rs1Val, ulong rs2Val, PipelineMicroOp op);
    }
}
