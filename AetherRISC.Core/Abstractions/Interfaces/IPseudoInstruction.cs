using System.Collections.Generic;
using AetherRISC.Core.Abstractions.Interfaces;

namespace AetherRISC.Core.Abstractions.Interfaces
{
    public interface IPseudoInstruction
    {
        string Mnemonic { get; }
        IEnumerable<IInstruction> Expand(int rd, int rs1, int rs2, long imm);
    }
}
