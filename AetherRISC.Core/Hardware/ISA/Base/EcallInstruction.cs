using System.Collections.Generic;
using AetherRISC.Core.Abstractions.Interfaces;
using AetherRISC.Core.Architecture;

namespace AetherRISC.Core.Hardware.ISA.Base;

public class EcallInstruction : IInstruction
{
    public string Mnemonic => "ECALL";
    public int Rd => 0; 
    public int Rs1 => 17; // Implicitly uses a7 (x17)
    public int Rs2 => 10; // Implicitly uses a0 (x10)
    public int Imm => 0;

    public bool IsLoad => false;
    public bool IsStore => false;
    public bool IsBranch => false;
    public bool IsJump => false;

    public void Execute(MachineState state)
    {
        // 1. Read a7 (x17) to get the Syscall ID
        long syscallId = (long)state.Registers.Read(17);

        if (state.Host == null) 
        {
             state.ProgramCounter += 4;
             return;
        }

        switch (syscallId)
        {
            case 1: // PrintInt
                long val = (long)state.Registers.Read(10); // Read a0
                state.Host.PrintInt(val);
                break;
            
            case 4: // PrintString
                ulong addr = state.Registers.Read(10); // Read address from a0
                string str = ReadStringFromMemory(state, addr);
                state.Host.PrintString(str);
                break;
            
            case 10: // Exit
                state.Host.Exit(0);
                break;
                
            case 93: // Exit2
                int code = (int)state.Registers.Read(10);
                state.Host.Exit(code);
                break;
        }

        state.ProgramCounter += 4;
    }

    private string ReadStringFromMemory(MachineState state, ulong address)
    {
        if (state.Memory == null) return "";
        
        var chars = new List<char>();
        while (true)
        {
            byte b = state.Memory.ReadByte((uint)address);
            if (b == 0) break; // Null terminator
            chars.Add((char)b);
            address++;
        }
        return new string(chars.ToArray());
    }
}
