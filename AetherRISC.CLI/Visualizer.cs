using System;
using System.Collections.Generic;
using System.Linq;
using AetherRISC.Core.Architecture;
using AetherRISC.Core.Architecture.Pipeline;

namespace AetherRISC.CLI;

public static class Visualizer
{
    public static List<string> OutputLog { get; } = new List<string>();

    public static void RenderPipeline(PipelineController pipe, MachineState state, long cycle)
    {
        Console.SetCursorPosition(0, 0);
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"╔══════════ Cycle: {cycle,-5} ══════════════════════════════════════════════════╗");
        Console.ResetColor();

        // --- PIPELINE STAGES ---
        PrintStage("IF ", pipe.IfId.PC, pipe.IfId.Instruction, 
            pipe.IfId.IsValid ? "FETCHING" : "STALL/FLUSH");

        var idInst = pipe.IdEx.DecodedInst;
        string idStr = idInst != null ? $"{idInst.Mnemonic} (rd:x{pipe.IdEx.Rd})" : "BUBBLE";
        PrintStage("ID ", pipe.IdEx.PC, pipe.IdEx.RawInstruction, idStr);

        var exInst = pipe.ExMem.DecodedInst;
        string exStr = exInst != null ? exInst.Mnemonic : "BUBBLE";
        if(exInst != null && !exInst.IsBranch && !exInst.IsStore) exStr += $" Res:{pipe.ExMem.AluResult:X}";
        PrintStage("EX ", pipe.ExMem.PC, pipe.ExMem.RawInstruction, exStr);

        var memInst = pipe.MemWb.DecodedInst;
        string memStr = memInst != null ? memInst.Mnemonic : "BUBBLE";
        if (pipe.MemWb.RegWrite) memStr += $" WB:x{pipe.MemWb.Rd}={pipe.MemWb.FinalResult:X}";
        PrintStage("MEM", pipe.MemWb.PC, pipe.MemWb.RawInstruction, memStr);

        // --- ALL REGISTERS (x0-x31) ---
        Console.WriteLine("╠══════════════════════════════════════════════════════════════════════════════╣");
        for (int i = 0; i < 32; i += 4)
        {
            Console.Write("║ ");
            for (int j = 0; j < 4; j++)
            {
                int reg = i + j;
                ulong val = state.Registers.Read(reg);
                Console.Write($"x{reg,-2}:{val:X8} "); // Compact hex
            }
            Console.WriteLine("║");
        }

        // --- LOG ---
        Console.WriteLine("╠══════════════════════════════════════════════════════════════════════════════╣");
        Console.ForegroundColor = ConsoleColor.Yellow;
        if (OutputLog.Count == 0) Console.WriteLine(" [LOG IS EMPTY]");
        foreach (var log in OutputLog.TakeLast(5))
        {
            Console.WriteLine($" {log.PadRight(80)}");
        }
        Console.ResetColor();
        Console.WriteLine("╚══════════════════════════════════════════════════════════════════════════════╝");
        Console.WriteLine(" [A] Auto-Run | [ENTER] Step | [Q] Quit");
    }

    private static void PrintStage(string stage, ulong pc, uint raw, string info)
    {
        Console.Write("║ [");
        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write(stage);
        Console.ResetColor();
        Console.WriteLine($"] PC:{pc:X4} [{raw:X8}] : {info,-45} ║");
    }
}
