using System.Collections.Generic;
using AetherRISC.Core.Abstractions.Interfaces;
using AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64I;

namespace AetherRISC.Core.Architecture.Hardware.ISA.Pseudo.Instructions;

public class BgtPseudo : IPseudoInstruction 
{ 
    public string Mnemonic => "BGT";
    public string Name => "Branch Greater Than";
    public string Description => "Branches if rs1 > rs2 (signed).";
    public string Usage => "bgt rs1, rs2, label";
    public IEnumerable<IInstruction> Expand(int rd, int rs1, int rs2, long imm) => [new BltInstruction(rs2, rs1, (int)imm)]; 
}

public class BlePseudo : IPseudoInstruction 
{ 
    public string Mnemonic => "BLE"; 
    public string Name => "Branch Less or Equal";
    public string Description => "Branches if rs1 <= rs2 (signed).";
    public string Usage => "ble rs1, rs2, label";
    public IEnumerable<IInstruction> Expand(int rd, int rs1, int rs2, long imm) => [new BgeInstruction(rs2, rs1, (int)imm)]; 
}

public class BgtuPseudo : IPseudoInstruction 
{ 
    public string Mnemonic => "BGTU"; 
    public string Name => "Branch Greater Than Unsigned";
    public string Description => "Branches if rs1 > rs2 (unsigned).";
    public string Usage => "bgtu rs1, rs2, label";
    public IEnumerable<IInstruction> Expand(int rd, int rs1, int rs2, long imm) => [new BltuInstruction(rs2, rs1, (int)imm)]; 
}

public class BleuPseudo : IPseudoInstruction 
{ 
    public string Mnemonic => "BLEU"; 
    public string Name => "Branch Less or Equal Unsigned";
    public string Description => "Branches if rs1 <= rs2 (unsigned).";
    public string Usage => "bleu rs1, rs2, label";
    public IEnumerable<IInstruction> Expand(int rd, int rs1, int rs2, long imm) => [new BgeuInstruction(rs2, rs1, (int)imm)]; 
}

public class BeqzPseudo : IPseudoInstruction 
{ 
    public string Mnemonic => "BEQZ"; 
    public string Name => "Branch Equal Zero";
    public string Description => "Branches if rs1 == 0.";
    public string Usage => "beqz rs1, label";
    public IEnumerable<IInstruction> Expand(int rd, int rs1, int rs2, long imm) => [new BeqInstruction(rs1, 0, (int)imm)]; 
}

public class BnezPseudo : IPseudoInstruction 
{ 
    public string Mnemonic => "BNEZ"; 
    public string Name => "Branch Not Equal Zero";
    public string Description => "Branches if rs1 != 0.";
    public string Usage => "bnez rs1, label";
    public IEnumerable<IInstruction> Expand(int rd, int rs1, int rs2, long imm) => [new BneInstruction(rs1, 0, (int)imm)]; 
}

public class BlezPseudo : IPseudoInstruction 
{ 
    public string Mnemonic => "BLEZ"; 
    public string Name => "Branch Less or Equal Zero";
    public string Description => "Branches if rs1 <= 0 (signed).";
    public string Usage => "blez rs1, label";
    public IEnumerable<IInstruction> Expand(int rd, int rs1, int rs2, long imm) => [new BgeInstruction(0, rs1, (int)imm)]; 
}

public class BgezPseudo : IPseudoInstruction 
{ 
    public string Mnemonic => "BGEZ"; 
    public string Name => "Branch Greater or Equal Zero";
    public string Description => "Branches if rs1 >= 0 (signed).";
    public string Usage => "bgez rs1, label";
    public IEnumerable<IInstruction> Expand(int rd, int rs1, int rs2, long imm) => [new BgeInstruction(rs1, 0, (int)imm)]; 
}

public class BltzPseudo : IPseudoInstruction 
{ 
    public string Mnemonic => "BLTZ"; 
    public string Name => "Branch Less Than Zero";
    public string Description => "Branches if rs1 < 0 (signed).";
    public string Usage => "bltz rs1, label";
    public IEnumerable<IInstruction> Expand(int rd, int rs1, int rs2, long imm) => [new BltInstruction(rs1, 0, (int)imm)]; 
}

public class BgtzPseudo : IPseudoInstruction 
{ 
    public string Mnemonic => "BGTZ"; 
    public string Name => "Branch Greater Than Zero";
    public string Description => "Branches if rs1 > 0 (signed).";
    public string Usage => "bgtz rs1, label";
    public IEnumerable<IInstruction> Expand(int rd, int rs1, int rs2, long imm) => [new BltInstruction(0, rs1, (int)imm)]; 
}
