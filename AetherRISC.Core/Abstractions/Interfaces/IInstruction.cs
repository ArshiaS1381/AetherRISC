using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.ISA; // Now holds InstructionData

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

    void Execute(MachineState state, InstructionData data);
}
