using System.Collections.Generic;
using AetherRISC.Core.Abstractions.Interfaces;
using AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64I;

namespace AetherRISC.Core.Architecture.Hardware.ISA.Pseudo.Instructions;

public class CallPseudo : IPseudoInstruction
{
    public string Mnemonic => "CALL";
    public string Name => "Call";
    public string Description => "Calls a function by jumping to a label and saving the return address in x1 (ra).";
    public string Usage => "call label";
    public IEnumerable<IInstruction> Expand(int rd, int rs1, int rs2, long imm) =>
        [new AuipcInstruction(1, 0), new JalrInstruction(1, 1, (int)imm)];
}

public class JPseudo : IPseudoInstruction 
{ 
    public string Mnemonic => "J"; 
    public string Name => "Jump";
    public string Description => "Unconditional jump to a label.";
    public string Usage => "j label";
    public IEnumerable<IInstruction> Expand(int rd, int rs1, int rs2, long imm) => [new JalInstruction(0, (int)imm)]; 
}

public class RetPseudo : IPseudoInstruction 
{ 
    public string Mnemonic => "RET"; 
    public string Name => "Return";
    public string Description => "Returns from a function (jumps to address in ra).";
    public string Usage => "ret";
    public IEnumerable<IInstruction> Expand(int rd, int rs1, int rs2, long imm) => [new JalrInstruction(0, 1, 0)]; 
}
