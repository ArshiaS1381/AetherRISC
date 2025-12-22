using AetherRISC.Core.Architecture;

namespace AetherRISC.Core.Abstractions.Interfaces;

public interface IInstruction
{
    string Mnemonic { get; }
    
    // Exposed Metadata for Pipeline
    int Rd { get; }      // Destination Register
    int Rs1 { get; }     // Source 1
    int Rs2 { get; }     // Source 2
    int Imm { get; }     // Immediate Value
    
    // Control Signals
    bool IsLoad { get; }
    bool IsStore { get; }
    bool IsBranch { get; }
    bool IsJump { get; }

    void Execute(MachineState state);
}
