using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using AetherRISC.CLI;
using AetherRISC.Core.Architecture.Simulation.Runners;

namespace AetherRISC.CLI
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "AetherRISC CLI";
            Console.OutputEncoding = Encoding.UTF8;
            
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                if (Console.WindowWidth < 100) Console.WindowWidth = 100;
                if (Console.WindowHeight < 40) Console.WindowHeight = 40;
            }

            ProgramLoader loader;
            try { loader = new ProgramLoader("config.json"); }
            catch (Exception ex) { Console.WriteLine($"Startup Error: {ex.Message}"); return; }

            while (true)
            {
                Console.Clear();
                DrawHeader();
                Console.WriteLine($" Config: [{loader.Config.Architecture.ToUpper()}] [{loader.Config.ExecutionMode.ToUpper()}] [{loader.Config.BranchPredictor.ToUpper()}] [Log:{loader.Config.LogLevel}]");
                Console.WriteLine("----------------------------------------");
                Console.WriteLine("  [1] Run Program");
                Console.WriteLine("  [2] Settings");
                Console.WriteLine("  [Q] Quit");
                Console.WriteLine("----------------------------------------");
                Console.Write(" Selection > ");

                var key = Console.ReadKey().Key;
                if (key == ConsoleKey.Q) break;
                if (key == ConsoleKey.D1 || key == ConsoleKey.NumPad1) ShowProgramList(loader);
                if (key == ConsoleKey.D2 || key == ConsoleKey.NumPad2) ShowSettings(loader);
            }
        }

        static void DrawHeader()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("========================================");
            Console.WriteLine("       AetherRISC Simulator v2.6");
            Console.WriteLine("========================================");
            Console.ResetColor();
        }

        static void ShowSettings(ProgramLoader loader)
        {
            while(true)
            {
                Console.Clear();
                DrawHeader();
                var c = loader.Config;
                Console.WriteLine(" Settings");
                Console.WriteLine("----------------------------------------");
                Console.WriteLine($"  [1] Architecture:   {c.Architecture.ToUpper()}");
                Console.WriteLine($"  [2] Execution Mode: {c.ExecutionMode.ToUpper()}");
                Console.WriteLine($"  [3] Stepping Mode:  {c.SteppingMode.ToUpper()}");
                Console.WriteLine($"  [4] Log Level:      {c.LogLevel}");
                Console.WriteLine($"  [5] Predictor:      {c.BranchPredictor.ToUpper()}");
                Console.WriteLine("  [B] Back");
                Console.WriteLine("----------------------------------------");
                Console.Write(" Toggle > ");

                var key = Console.ReadKey(true).Key;
                if (key == ConsoleKey.B) break;
                
                if (key == ConsoleKey.D1) c.Architecture = c.Architecture == "rv64" ? "rv32" : "rv64";
                if (key == ConsoleKey.D2) c.ExecutionMode = c.ExecutionMode == "pipeline" ? "simple" : "pipeline";
                if (key == ConsoleKey.D3) c.SteppingMode = c.SteppingMode == "auto" ? "manual" : "auto";
                if (key == ConsoleKey.D4) 
                {
                    c.LogLevel = c.LogLevel switch 
                    {
                        SimulationLogLevel.None => SimulationLogLevel.Simple,
                        SimulationLogLevel.Simple => SimulationLogLevel.Verbose,
                        SimulationLogLevel.Verbose => SimulationLogLevel.None,
                        _ => SimulationLogLevel.Simple
                    };
                }
                if (key == ConsoleKey.D5)
                {
                    c.BranchPredictor = c.BranchPredictor switch
                    {
                        "static" => "bimodal",
                        "bimodal" => "gshare",
                        "gshare" => "static",
                        _ => "static"
                    };
                }
                
                loader.SaveConfig();
            }
        }

        static void ShowProgramList(ProgramLoader loader)
        {
            var files = loader.GetAvailablePrograms();
            while(true)
            {
                Console.Clear();
                DrawHeader();
                Console.WriteLine(" Select Program:");
                if (files.Length == 0) Console.WriteLine("  (No files found)");
                for (int i = 0; i < files.Length; i++)
                    Console.WriteLine($"  [{i + 1}] {Path.GetFileName(files[i])}");
                
                Console.WriteLine("\n  [B] Back");
                Console.Write(" > ");
                
                var input = Console.ReadLine();
                if (input?.ToUpper() == "B") return;
                
                if (int.TryParse(input, out int id) && id > 0 && id <= files.Length)
                {
                    RunSimulation(loader, files[id - 1]);
                    Console.WriteLine("\nPress any key to continue...");
                    Console.ReadKey();
                    return;
                }
            }
        }

        static void RunSimulation(ProgramLoader loader, string file)
        {
            using var session = loader.PrepareSession(file);
            bool isManual = loader.Config.SteppingMode == "manual";
            bool isPipeline = loader.Config.ExecutionMode == "pipeline";
            
            int cycles = 0;
            Stopwatch sw = new Stopwatch();
            
            Console.Clear();
            sw.Start();

            while (!session.State.Halted)
            {
                if (isManual)
                {
                    sw.Stop();
                    RenderUI(session, cycles, isPipeline, sw.Elapsed);
                    var k = Console.ReadKey(true).Key;
                    sw.Start();

                    if (k == ConsoleKey.Q) { session.State.Halted = true; break; }
                    if (k == ConsoleKey.R) isManual = false; 
                }

                if (isPipeline) session.PipelinedRunner?.Step(cycles);
                else session.SimpleRunner?.Run(1);

                cycles++;
                if (cycles > loader.Config.MaxCycles) break;
                
                if (!isManual) 
                {
                    // Check every 5k cycles for UI updates or Pause requests
                    if (cycles % 5000 == 0) 
                    {
                        if (Console.KeyAvailable)
                        {
                            sw.Stop();
                            RenderUI(session, cycles, isPipeline, sw.Elapsed);
                            Console.ReadKey(true); 
                            
                            Console.SetCursorPosition(0, 30);
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine("\n[PAUSED] Switched to Manual Mode. Press Enter to Step, 'R' to Resume.");
                            Console.ResetColor();
                            
                            var nextKey = Console.ReadKey(true).Key;
                            if (nextKey == ConsoleKey.R) sw.Start();
                            else if (nextKey == ConsoleKey.Q) { session.State.Halted = true; break; }
                            else { isManual = true; sw.Start(); }
                        }
                        else RenderUI(session, cycles, isPipeline, sw.Elapsed);
                    }
                }
            }
            sw.Stop();
            
            RenderUI(session, cycles, isPipeline, sw.Elapsed);
            
            Console.SetCursorPosition(0, 32); 
            Console.WriteLine("\n----------------------------------------");
            Console.WriteLine(" Execution Report");
            Console.WriteLine("----------------------------------------");
            Console.WriteLine($" Status:       {(session.State.Halted ? "HALTED" : "TIMEOUT")}");
            Console.WriteLine($" Total Cycles: {cycles:N0}");
            Console.WriteLine($" Time Elapsed: {sw.Elapsed.TotalSeconds:F4} seconds");
            
            if (sw.Elapsed.TotalSeconds > 0.001)
            {
                double khz = (cycles / sw.Elapsed.TotalSeconds) / 1000.0;
                Console.WriteLine($" Avg Speed:    {khz:F2} KHz");
            }
        }

        static void RenderUI(SimulationSession s, int cycle, bool isPipeline, TimeSpan elapsed)
        {
            Console.SetCursorPosition(0, 0);
            
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(FormatLineTruncate($" CYCLE: {cycle,-10} | PC: 0x{s.State.Registers.PC:X8} | TIME: {elapsed.TotalSeconds:F2}s"));
            Console.ResetColor();
            
            Console.WriteLine(FormatLineTruncate(" [Registers]"));
            for(int i=0; i<32; i+=4)
            {
                string r1 = $"x{i}={s.State.Registers.Read(i):X8}";
                string r2 = $"x{i+1}={s.State.Registers.Read(i+1):X8}";
                string r3 = $"x{i+2}={s.State.Registers.Read(i+2):X8}";
                string r4 = $"x{i+3}={s.State.Registers.Read(i+3):X8}";
                Console.WriteLine(FormatLineTruncate($" {r1,-15} {r2,-15} {r3,-15} {r4,-15}"));
            }

            Console.WriteLine(FormatLineTruncate(" [Pipeline Stages]"));
            if (isPipeline && s.PipelinedRunner != null)
            {
                var pipe = s.PipelinedRunner.PipelineState;
                Console.WriteLine(FormatLineTruncate($" IF:  [{FormatHex(pipe.FetchDecode.Instruction)}] Stall:{pipe.FetchDecode.IsStalled} Pred:{pipe.FetchDecode.PredictedTaken}"));
                Console.WriteLine(FormatLineTruncate($" ID:  [{FormatHex(pipe.DecodeExecute.RawInstruction)}] {FormatMnemonic(pipe.DecodeExecute.DecodedInst?.Mnemonic)}"));
                Console.WriteLine(FormatLineTruncate($" EX:  [{FormatHex(pipe.ExecuteMemory.RawInstruction)}] {FormatMnemonic(pipe.ExecuteMemory.DecodedInst?.Mnemonic)} Mis:{pipe.ExecuteMemory.Misprediction}"));
                Console.WriteLine(FormatLineTruncate($" MEM: [{FormatHex(pipe.MemoryWriteback.RawInstruction)}] {FormatMnemonic(pipe.MemoryWriteback.DecodedInst?.Mnemonic)}"));
                Console.WriteLine(FormatLineTruncate($" WB:  RegWrite:{pipe.MemoryWriteback.RegWrite} Rd:{pipe.MemoryWriteback.Rd} Val:{pipe.MemoryWriteback.FinalResult:X}"));
            }
            else
            {
                for(int i=0; i<5; i++) Console.WriteLine(FormatLineTruncate(""));
            }

            // ... (Rest of existing UI code) ...
            Console.WriteLine(FormatLineTruncate(" [Console Output]"));
            Console.BackgroundColor = ConsoleColor.DarkBlue;
            Console.ForegroundColor = ConsoleColor.White;
            
            var consoleLines = GetWrappedLines(s.OutputBuffer.ToString(), Console.WindowWidth - 2);
            int totalLines = consoleLines.Count;
            int start = Math.Max(0, totalLines - 5);
            
            for(int i=0; i<5; i++) 
            {
                string content = (start + i < totalLines) ? consoleLines[start + i] : "";
                Console.WriteLine(FormatLineTruncate(" " + content));
            }
            Console.ResetColor();

            Console.WriteLine(FormatLineTruncate(" [Controls] Enter: Step | R: Run All | Q: Quit"));
        }

        static List<string> GetWrappedLines(string fullText, int width)
        {
            var result = new List<string>();
            var logicalLines = fullText.Replace("\r\n", "\n").Split('\n');
            
            foreach (var line in logicalLines)
            {
                if (string.IsNullOrEmpty(line))
                {
                    result.Add("");
                    continue;
                }
                for (int i = 0; i < line.Length; i += width)
                {
                    int len = Math.Min(width, line.Length - i);
                    result.Add(line.Substring(i, len));
                }
            }
            return result;
        }

        static string FormatLineTruncate(string text)
        {
            int w = Console.WindowWidth - 1; 
            if (text.Length > w) return text.Substring(0, w);
            return text.PadRight(w);
        }

        static string FormatMnemonic(string? m) => (m ?? "-").PadRight(10);
        static string FormatHex(uint val) => val == 0 ? "        " : $"{val:X8}";
    }
}
