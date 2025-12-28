using System.Collections.Generic;
using AetherRISC.Core.Abstractions.Interfaces;


namespace AetherRISC.Core.Architecture.Hardware.ISA.Pseudo.Instructions;

public class JPseudo : IPseudoInstruction 
{ 
    public string Mnemonic => "J"; 
    public IEnumerable<IInstruction> Expand(int rd, int rs1, int rs2, long imm) => [new JalInstruction(0, (int)imm)]; 
}

public class RetPseudo : IPseudoInstruction 
{ 
    public string Mnemonic => "RET"; 
    public IEnumerable<IInstruction> Expand(int rd, int rs1, int rs2, long imm) => [new JalrInstruction(0, 1, 0)]; 
}
