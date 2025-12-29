using System.Collections.Generic;
using AetherRISC.Core.Abstractions.Interfaces;
using AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64I;

namespace AetherRISC.Core.Architecture.Hardware.ISA.Pseudo.Instructions;

public class SeqzPseudo : IPseudoInstruction 
{ 
    public string Mnemonic => "SEQZ"; 
    public string Name => "Set Equal Zero";
    public string Description => "Sets rd to 1 if rs1 == 0, else 0.";
    public string Usage => "seqz rd, rs1";
    public IEnumerable<IInstruction> Expand(int rd, int rs1, int rs2, long imm) => [new SltiuInstruction(rd, rs1, 1)]; 
}

public class SnezPseudo : IPseudoInstruction 
{ 
    public string Mnemonic => "SNEZ"; 
    public string Name => "Set Not Equal Zero";
    public string Description => "Sets rd to 1 if rs1 != 0, else 0.";
    public string Usage => "snez rd, rs1";
    public IEnumerable<IInstruction> Expand(int rd, int rs1, int rs2, long imm) => [new SltuInstruction(rd, 0, rs1)]; 
}

public class SltzPseudo : IPseudoInstruction 
{ 
    public string Mnemonic => "SLTZ"; 
    public string Name => "Set Less Than Zero";
    public string Description => "Sets rd to 1 if rs1 < 0, else 0.";
    public string Usage => "sltz rd, rs1";
    public IEnumerable<IInstruction> Expand(int rd, int rs1, int rs2, long imm) => [new SltInstruction(rd, rs1, 0)]; 
}

public class SgtzPseudo : IPseudoInstruction 
{ 
    public string Mnemonic => "SGTZ"; 
    public string Name => "Set Greater Than Zero";
    public string Description => "Sets rd to 1 if rs1 > 0, else 0.";
    public string Usage => "sgtz rd, rs1";
    public IEnumerable<IInstruction> Expand(int rd, int rs1, int rs2, long imm) => [new SltInstruction(rd, 0, rs1)]; 
}
