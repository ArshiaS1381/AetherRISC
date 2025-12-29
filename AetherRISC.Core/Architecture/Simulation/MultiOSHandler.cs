using System;
using System.IO;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Abstractions.Interfaces;

namespace AetherRISC.Core.Architecture.Simulation;

public enum OSKind { RARS, Linux }

public class MultiOSHandler : IHostSystem
{
    public OSKind Kind { get; set; } = OSKind.RARS;
    public TextWriter Output { get; set; } = Console.Out;
    public bool Silent { get; set; } = false;
    
    private uint _heapPointer = 0x10040000;
    private readonly Random _random = new();

    public void HandleEcall(MachineState state)
    {
        if (Kind == OSKind.RARS) HandleRars(state);
        else HandleLinux(state);
    }

    public void HandleBreak(MachineState state) 
    {
        if (!Silent) Output.WriteLine($"[Debugger] Breakpoint at 0x{state.ProgramCounter:X}");
        
        // [FIX] EBREAK must halt the simulation in test contexts
        state.Halted = true;
    }

    private void HandleRars(MachineState state)
    {
        int a7 = (int)state.Registers.Read(17);
        switch (a7)
        {
            case 1: Output.Write((long)state.Registers.Read(10)); break;
            case 4: PrintNullTerminated(state, (uint)state.Registers.Read(10)); break;
            case 5:
                string? s = Silent ? "0" : Console.ReadLine();
                if (int.TryParse(s, out int ri)) state.Registers.Write(10, (ulong)ri);
                break;
            case 10: state.Halted = true; break; 
            case 11: Output.Write((char)state.Registers.Read(10)); break;
            case 30: state.Registers.Write(10, (ulong)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()); break;
            case 93: if (!Silent) Environment.Exit((int)state.Registers.Read(10)); break;
        }
    }

    private void HandleLinux(MachineState state)
    {
        int a7 = (int)state.Registers.Read(17);
        switch (a7)
        {
            case 64: // write
                int wfd = (int)state.Registers.Read(10);
                uint wbuf = (uint)state.Registers.Read(11);
                int wcnt = (int)state.Registers.Read(12);
                if (wfd == 1 || wfd == 2) { PrintString(state, wbuf, wcnt); state.Registers.Write(10, (ulong)wcnt); }
                break;
            case 93: case 94: if (!Silent) Environment.Exit((int)state.Registers.Read(10)); break;
            case 214: // brk
                ulong nb = state.Registers.Read(10);
                if (nb == 0) state.Registers.Write(10, _heapPointer);
                else { _heapPointer = (uint)nb; state.Registers.Write(10, nb); }
                break;
        }
    }

    private void PrintNullTerminated(MachineState s, uint addr) {
        if (s.Memory == null) return;
        char c;
        while ((c = (char)s.Memory.ReadByte(addr++)) != 0) Output.Write(c);
    }

    private void PrintString(MachineState s, uint addr, int len) {
        if (s.Memory == null) return;
        for(int i=0; i<len; i++) Output.Write((char)s.Memory.ReadByte(addr + (uint)i));
    }
}
