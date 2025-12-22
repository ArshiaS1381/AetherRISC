using AetherRISC.Core.Abstractions.Interfaces;
using AetherRISC.Core.Architecture;

namespace AetherRISC.Core.Hardware.ISA.Base;

public class LuiInstruction : IInstruction
{
    public string Mnemonic => "LUI";
    public int Rd { get; }
    public int Rs1 => 0;
    public int Rs2 => 0;
    public int Imm { get; } // 20-bit upper immediate

    public bool IsLoad => false;
    public bool IsStore => false;
    public bool IsBranch => false;
    public bool IsJump => false;

    public LuiInstruction(int rd, int imm)
    {
        Rd = rd;
        Imm = imm;
    }

    public void Execute(MachineState state)
    {
        // LUI places the 20-bit immediate into bits 31-12 of Rd, filling the lower 12 bits with zeros.
        // In C#, we shift it up during decoding or here. 
        // Standard RISC-V convention: The immediate passed here is usually already shifted 
        // if coming from the decoder, OR it is the raw top 20 bits.
        // Let's assume the Decoder passes the *shifted* value (32-bit integer).
        
        state.Registers.Write(Rd, (ulong)Imm); 
        state.ProgramCounter += 4;
    }
}
