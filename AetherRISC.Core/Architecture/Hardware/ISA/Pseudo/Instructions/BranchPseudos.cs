using System.Collections.Generic;
using AetherRISC.Core.Abstractions.Interfaces;

namespace AetherRISC.Core.Architecture.Hardware.ISA.Pseudo.Instructions;

public class BgtPseudo : IPseudoInstruction { public string Mnemonic => "BGT"; public IEnumerable<IInstruction> Expand(int rd, int rs1, int rs2, long imm) => [new BltInstruction(rs2, rs1, (int)imm)]; }
public class BlePseudo : IPseudoInstruction { public string Mnemonic => "BLE"; public IEnumerable<IInstruction> Expand(int rd, int rs1, int rs2, long imm) => [new BgeInstruction(rs2, rs1, (int)imm)]; }
public class BgtuPseudo : IPseudoInstruction { public string Mnemonic => "BGTU"; public IEnumerable<IInstruction> Expand(int rd, int rs1, int rs2, long imm) => [new BltuInstruction(rs2, rs1, (int)imm)]; }
public class BleuPseudo : IPseudoInstruction { public string Mnemonic => "BLEU"; public IEnumerable<IInstruction> Expand(int rd, int rs1, int rs2, long imm) => [new BgeuInstruction(rs2, rs1, (int)imm)]; }

public class BeqzPseudo : IPseudoInstruction { public string Mnemonic => "BEQZ"; public IEnumerable<IInstruction> Expand(int rd, int rs1, int rs2, long imm) => [new BeqInstruction(rs1, 0, (int)imm)]; }
public class BnezPseudo : IPseudoInstruction { public string Mnemonic => "BNEZ"; public IEnumerable<IInstruction> Expand(int rd, int rs1, int rs2, long imm) => [new BneInstruction(rs1, 0, (int)imm)]; }
public class BlezPseudo : IPseudoInstruction { public string Mnemonic => "BLEZ"; public IEnumerable<IInstruction> Expand(int rd, int rs1, int rs2, long imm) => [new BgeInstruction(0, rs1, (int)imm)]; }
public class BgezPseudo : IPseudoInstruction { public string Mnemonic => "BGEZ"; public IEnumerable<IInstruction> Expand(int rd, int rs1, int rs2, long imm) => [new BgeInstruction(rs1, 0, (int)imm)]; }
public class BltzPseudo : IPseudoInstruction { public string Mnemonic => "BLTZ"; public IEnumerable<IInstruction> Expand(int rd, int rs1, int rs2, long imm) => [new BltInstruction(rs1, 0, (int)imm)]; }
public class BgtzPseudo : IPseudoInstruction { public string Mnemonic => "BGTZ"; public IEnumerable<IInstruction> Expand(int rd, int rs1, int rs2, long imm) => [new BltInstruction(0, rs1, (int)imm)]; }
