using System;
using System.Collections.Generic;
using System.Linq;
using AetherRISC.Core.Architecture;
using AetherRISC.Core.Architecture.Pipeline;
using AetherRISC.Core.Abstractions.Interfaces;

namespace AetherRISC.CLI;

public static class Visualizer
{
    public static List<string> OutputLog { get; } = new List<string>();

    public static void RenderPipeline(PipelineController pipe, MachineState state, long cycle)
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"╔══════════ Cycle: {cycle,-5} ══════════════════════════════════════════════════╗");
        Console.ResetColor();

        // --- PIPELINE STAGES ---
        
        // 1. FETCH (Output of IF)
        PrintStage("IF ", pipe.IfId.PC, pipe.IfId.Instruction, "FETCH");

        // 2. DECODE (Output of ID)
        var idInst = pipe.IdEx.DecodedInst;
        string idStr = idInst != null ? $"{idInst.Mnemonic} (rd:{pipe.IdEx.Rd})" : "BUBBLE";
        PrintStage("ID ", pipe.IdEx.PC, pipe.IdEx.RawInstruction, idStr);

        // 3. EXECUTE (Output of EX)
        var exInst = pipe.ExMem.DecodedInst;
        string exStr = exInst != null ? exInst.Mnemonic : "BUBBLE";
        PrintStage("EX ", pipe.ExMem.PC, pipe.ExMem.RawInstruction, exStr);

        // 4. MEMORY (Output of MEM)
        var memInst = pipe.MemWb.DecodedInst; // MemWb holds the output of Memory stage
        string memStr = memInst != null ? memInst.Mnemonic : "BUBBLE";
        PrintStage("MEM", pipe.MemWb.PC, pipe.MemWb.RawInstruction, memStr);

        // 5. WRITEBACK (Committing)
        // Since MemWb is the input to Writeback, we display it here too, but focus on the write action
        string wbStr = pipe.MemWb.RegWrite ? $"Write x{pipe.MemWb.Rd} = {pipe.MemWb.FinalResult}" : "PASS";
        PrintStage("WB ", pipe.MemWb.PC, pipe.MemWb.RawInstruction, wbStr);

        Console.WriteLine("╠══════════════════════════════════════════════════════════════════════════════╣");

        // --- REGISTERS ---
        for (int i = 0; i < 32; i += 2)
        {
            PrintRegPair(state, i, i+1);
        }
        
        Console.WriteLine("╠══════════════════════════════════════════════════════════════════════════════╣");

        // --- LOG ---
        Console.ForegroundColor = ConsoleColor.Yellow;
        if (OutputLog.Count == 0) Console.WriteLine(" [LOG IS EMPTY]");
        foreach (var log in OutputLog.TakeLast(5))
        {
            Console.WriteLine($" {log}");
        }
        Console.ResetColor();
        
        Console.WriteLine("╚══════════════════════════════════════════════════════════════════════════════╝");
        Console.WriteLine(" Controls: [ENTER] Cycle | [q] Quit");
    }

    private static void PrintStage(string stage, ulong pc, uint raw, string info)
    {
        Console.Write("║ [");
        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write(stage);
        Console.ResetColor();
        Console.WriteLine($"]: Inst [{raw:X8}]@[{pc:X8}]: {info,-30} ║");
    }

    private static void PrintRegPair(MachineState state, int r1, int r2)
    {
        ulong v1 = state.Registers.Read(r1);
        ulong v2 = state.Registers.Read(r2);
        
        Console.Write("║ ");
        FormatReg(r1, v1);
        Console.Write(" │ ");
        FormatReg(r2, v2);
        Console.WriteLine(" ║");
    }

    private static void FormatReg(int reg, ulong val)
    {
        Console.Write($"x{reg,-2}: ");
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write($"{val:X8}");
        Console.ResetColor();
        Console.Write($" ({(long)val,12})");
    }
}
