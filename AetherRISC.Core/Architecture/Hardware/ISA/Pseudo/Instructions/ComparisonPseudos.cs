using System.Collections.Generic;
using AetherRISC.Core.Abstractions.Interfaces;


namespace AetherRISC.Core.Architecture.Hardware.ISA.Pseudo.Instructions;

public class SeqzPseudo : IPseudoInstruction { public string Mnemonic => "SEQZ"; public IEnumerable<IInstruction> Expand(int rd, int rs1, int rs2, long imm) => [new SltiuInstruction(rd, rs1, 1)]; }
public class SnezPseudo : IPseudoInstruction { public string Mnemonic => "SNEZ"; public IEnumerable<IInstruction> Expand(int rd, int rs1, int rs2, long imm) => [new SltuInstruction(rd, 0, rs1)]; }
public class SltzPseudo : IPseudoInstruction { public string Mnemonic => "SLTZ"; public IEnumerable<IInstruction> Expand(int rd, int rs1, int rs2, long imm) => [new SltInstruction(rd, rs1, 0)]; }
public class SgtzPseudo : IPseudoInstruction { public string Mnemonic => "SGTZ"; public IEnumerable<IInstruction> Expand(int rd, int rs1, int rs2, long imm) => [new SltInstruction(rd, 0, rs1)]; }
