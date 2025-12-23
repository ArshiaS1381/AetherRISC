using AetherRISC.Core.Architecture;

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

        void Execute(MachineState state, InstructionData data);
    }
}
