using System.Collections.Generic;
using AetherRISC.Core.Abstractions.Interfaces;
using AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64I;

namespace AetherRISC.Core.Architecture.Hardware.ISA.Pseudo.Instructions
{
    public class NopPseudo : IPseudoInstruction 
    { 
        public string Mnemonic => "NOP"; 
        public string Name => "No Operation";
        public string Description => "Performs no operation.";
        public string Usage => "nop";
        public IEnumerable<IInstruction> Expand(int rd, int rs1, int rs2, long imm) => [new AddiInstruction(0, 0, 0)]; 
    }

    public class MvPseudo : IPseudoInstruction 
    { 
        public string Mnemonic => "MV"; 
        public string Name => "Move";
        public string Description => "Copies value from rs1 to rd.";
        public string Usage => "mv rd, rs1";
        public IEnumerable<IInstruction> Expand(int rd, int rs1, int rs2, long imm) => [new AddiInstruction(rd, rs1, 0)]; 
    }

    public class NotPseudo : IPseudoInstruction 
    { 
        public string Mnemonic => "NOT"; 
        public string Name => "Not";
        public string Description => "Bitwise logical negation (One's Complement).";
        public string Usage => "not rd, rs1";
        public IEnumerable<IInstruction> Expand(int rd, int rs1, int rs2, long imm) => [new XoriInstruction(rd, rs1, -1)]; 
    }

    public class NegPseudo : IPseudoInstruction 
    { 
        public string Mnemonic => "NEG"; 
        public string Name => "Negate";
        public string Description => "Two's complement negation (0 - rs1).";
        public string Usage => "neg rd, rs1";
        public IEnumerable<IInstruction> Expand(int rd, int rs1, int rs2, long imm) => [new SubInstruction(rd, 0, rs1)]; 
    }

    public class NegwPseudo : IPseudoInstruction 
    { 
        public string Mnemonic => "NEGW"; 
        public string Name => "Negate Word";
        public string Description => "Two's complement negation on lower 32 bits.";
        public string Usage => "negw rd, rs1";
        public IEnumerable<IInstruction> Expand(int rd, int rs1, int rs2, long imm) => [new SubwInstruction(rd, 0, rs1)]; 
    }

    public class SextwPseudo : IPseudoInstruction 
    { 
        public string Mnemonic => "SEXT.W"; 
        public string Name => "Sign Extend Word";
        public string Description => "Sign-extends the lower 32 bits of rs1 to 64 bits.";
        public string Usage => "sext.w rd, rs1";
        public IEnumerable<IInstruction> Expand(int rd, int rs1, int rs2, long imm) => [new AddiwInstruction(rd, rs1, 0)]; 
    }
}
