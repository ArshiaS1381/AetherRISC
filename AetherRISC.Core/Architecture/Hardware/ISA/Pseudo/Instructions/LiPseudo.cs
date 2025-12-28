using System.Collections.Generic;
using AetherRISC.Core.Abstractions.Interfaces;

namespace AetherRISC.Core.Architecture.Hardware.ISA.Pseudo.Instructions;

public class LiPseudo : IPseudoInstruction
{
    public string Mnemonic => "LI";
    public IEnumerable<IInstruction> Expand(int rd, int rs1, int rs2, long imm)
    {
        // FORCE 2 instructions for consistency if it might have been estimated as 8 bytes.
        // However, Assembler can predict LI size for *numbers*. It fails for *Labels*.
        // If 'imm' came from a label resolution, we must match Pass 1.
        // Since we can't distinguish here, and robust labels are critical, we disable optimization 
        // for large ranges or ambiguous cases. 
        
        // Note: For simple small constants (-2048 to 2047), the Assembler Pass 1 
        // DOES predict 4 bytes (see SourceAssembler.cs). So we MUST keep 1 instruction there.
        
        if (imm >= -2048 && imm <= 2047)
            return [new AddiInstruction(rd, 0, (int)imm)];

        // For anything larger (or what Pass 1 assumed was large/label), use 2 instrs.
        int lower = (int)(imm & 0xFFF);
        if ((lower & 0x800) != 0) lower -= 4096;
        int upper = (int)((imm - lower) & ~0xFFFL);
        return [new LuiInstruction(rd, upper), new AddiwInstruction(rd, rd, lower)];
    }
}
