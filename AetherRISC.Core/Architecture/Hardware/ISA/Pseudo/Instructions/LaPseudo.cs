using System.Collections.Generic;
using AetherRISC.Core.Abstractions.Interfaces;

namespace AetherRISC.Core.Architecture.Hardware.ISA.Pseudo.Instructions;

public class LaPseudo : IPseudoInstruction
{
    public string Mnemonic => "LA";
    public IEnumerable<IInstruction> Expand(int rd, int rs1, int rs2, long imm)
    {
        // FORCE 2 instructions to match Assembler Pass 1 estimation (8 bytes).
        // Optimizing to 1 instruction breaks label offsets if Pass 1 assumed 8 bytes.
        
        int lower = (int)(imm & 0xFFF);
        if ((lower & 0x800) != 0) lower -= 4096;
        int upper = (int)((imm - (long)lower) & ~0xFFFL);
        
        // Even if upper is 0, we emit LUI to maintain alignment/size.
        // LUI x, 0 is effectively a NOP for the upper bits but clears the reg, which is fine before ADDI.
        return [new LuiInstruction(rd, upper), new AddiInstruction(rd, rd, lower)];
    }
}
