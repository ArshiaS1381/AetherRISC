using AetherRISC.Core.Architecture.ISA.Decoding;

namespace AetherRISC.Core.Abstractions.Interfaces
{
    public interface IInstructionFamily
    {
        // Called by the CPU during startup to load this extension
        void Register(InstructionDecoder decoder);
    }
}
