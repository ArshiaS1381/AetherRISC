using System.Collections.Generic;
using AetherRISC.Core.Abstractions.Interfaces;
using AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64I;

namespace AetherRISC.Core.Architecture.Hardware.ISA.Pseudo.Instructions;

public class LaPseudo : IPseudoInstruction
{
    public string Mnemonic => "LA";
    public string Name => "Load Address";
    public string Description => "Loads the absolute address of a symbol into rd.";
    public string Usage => "la rd, label";
    public IEnumerable<IInstruction> Expand(int rd, int rs1, int rs2, long imm)
    {
        int lower = (int)(imm & 0xFFF);
        if ((lower & 0x800) != 0) lower -= 4096;
        int upper = (int)((imm - (long)lower) & ~0xFFFL);
        return [new LuiInstruction(rd, upper), new AddiInstruction(rd, rd, lower)];
    }
}

public class LiPseudo : IPseudoInstruction
{
    public string Mnemonic => "LI";
    public string Name => "Load Immediate";
    public string Description => "Loads an immediate value into rd.";
    public string Usage => "li rd, imm";
    public IEnumerable<IInstruction> Expand(int rd, int rs1, int rs2, long imm)
    {
        if (imm >= -2048 && imm <= 2047)
            return [new AddiInstruction(rd, 0, (int)imm)];

        int lower = (int)(imm & 0xFFF);
        if ((lower & 0x800) != 0) lower -= 4096;
        int upper = (int)((imm - lower) & ~0xFFFL);
        return [new LuiInstruction(rd, upper), new AddiwInstruction(rd, rd, lower)];
    }
}
