using System.Collections.Generic;
using AetherRISC.Core.Abstractions.Interfaces;


namespace AetherRISC.Core.Architecture.Hardware.ISA.Pseudo.Instructions;

public class CallPseudo : IPseudoInstruction
{
    public string Mnemonic => "CALL";
    public IEnumerable<IInstruction> Expand(int rd, int rs1, int rs2, long imm) =>
        [new AuipcInstruction(1, 0), new JalrInstruction(1, 1, (int)imm)];
}
