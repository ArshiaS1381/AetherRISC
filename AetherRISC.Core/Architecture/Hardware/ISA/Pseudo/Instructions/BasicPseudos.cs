using System.Collections.Generic;
using AetherRISC.Core.Abstractions.Interfaces;

using AetherRISC.Core.Architecture.Hardware.ISA.Extensions.B.Zbb; // For ZEXT.H if available

namespace AetherRISC.Core.Architecture.Hardware.ISA.Pseudo.Instructions;

public class NopPseudo : IPseudoInstruction { public string Mnemonic => "NOP"; public IEnumerable<IInstruction> Expand(int rd, int rs1, int rs2, long imm) => [new AddiInstruction(0, 0, 0)]; }
public class MvPseudo : IPseudoInstruction { public string Mnemonic => "MV"; public IEnumerable<IInstruction> Expand(int rd, int rs1, int rs2, long imm) => [new AddiInstruction(rd, rs1, 0)]; }
public class NotPseudo : IPseudoInstruction { public string Mnemonic => "NOT"; public IEnumerable<IInstruction> Expand(int rd, int rs1, int rs2, long imm) => [new XoriInstruction(rd, rs1, -1)]; }
public class NegPseudo : IPseudoInstruction { public string Mnemonic => "NEG"; public IEnumerable<IInstruction> Expand(int rd, int rs1, int rs2, long imm) => [new SubInstruction(rd, 0, rs1)]; }
public class NegwPseudo : IPseudoInstruction { public string Mnemonic => "NEGW"; public IEnumerable<IInstruction> Expand(int rd, int rs1, int rs2, long imm) => [new SubwInstruction(rd, 0, rs1)]; }
public class SextwPseudo : IPseudoInstruction { public string Mnemonic => "SEXT.W"; public IEnumerable<IInstruction> Expand(int rd, int rs1, int rs2, long imm) => [new AddiwInstruction(rd, rs1, 0)]; }
public class ZexthPseudo : IPseudoInstruction { public string Mnemonic => "ZEXT.H"; public IEnumerable<IInstruction> Expand(int rd, int rs1, int rs2, long imm) => [new ZextHInstruction(rd, rs1, 0)]; }
