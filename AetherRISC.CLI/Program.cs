using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using AetherRISC.CLI;
using AetherRISC.Core.Architecture.Simulation.Runners;
using AetherRISC.Core.Abstractions.Diagnostics;

namespace AetherRISC.CLI
{
    class Program
    {
        // Global view state
        static int _consoleScroll = 0;
        static int _windowHeight = 50;
        static int _windowWidth = 120;

        static void Main(string[] args)
        {
            SetupConsole();

            ProgramLoader loader;
            try { loader = new ProgramLoader("config.json"); }
            catch (Exception ex) { Console.WriteLine($"Startup Error: {ex.Message}"); return; }

            // --- MAIN MENU LOOP ---
            while (true)
            {
                // Prepare Menu Items
                string predShort = loader.Config.BranchPredictor.ToUpper().Replace("BIMODAL", "BIM"); 
                string earlyRes = loader.Config.EnableEarlyBranchResolution ? "FAST" : "ACC";
                string rtMetrics = loader.Config.ShowRealTimeMetrics ? "ON" : "OFF";
                string configStr = $"[{loader.Config.Architecture.ToUpper()}] [{loader.Config.ExecutionMode.ToUpper()}] [BP:{predShort}/{loader.Config.PredictorInitValue}] [{earlyRes}] [RT:{rtMetrics}]";

                int selected = MenuHandler.Show("AetherRISC Simulator v3.8", configStr, new[] { "Run Program", "Settings", "Quit" });

                if (selected == 0) ShowProgramList(loader);
                else if (selected == 1) ShowSettings(loader);
                else if (selected == 2) break;
            }
        }

        static void SetupConsole()
        {
            Console.Title = "AetherRISC CLI";
            Console.OutputEncoding = Encoding.UTF8;
            Console.CursorVisible = false;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                if (Console.WindowWidth < _windowWidth) Console.WindowWidth = _windowWidth;
                if (Console.WindowHeight < _windowHeight) Console.WindowHeight = _windowHeight;
                if (Console.BufferHeight < _windowHeight) Console.BufferHeight = _windowHeight;
                if (Console.BufferWidth < _windowWidth) Console.BufferWidth = _windowWidth;
            }
        }

        static void ShowSettings(ProgramLoader loader)
        {
            int selected = 0;
            while(true)
            {
                var c = loader.Config;
                string[] options = new[] 
                {
                    $"Architecture:   {c.Architecture.ToUpper()}",
                    $"Execution Mode: {c.ExecutionMode.ToUpper()}",
                    $"Stepping Mode:  {c.SteppingMode.ToUpper()}",
                    $"Log Level:      {c.LogLevel}",
                    $"Predictor Type: {c.BranchPredictor.ToUpper()}",
                    $"Predictor Init: {c.PredictorInitValue}",
                    $"Resolution:     {(c.EnableEarlyBranchResolution ? "Fast (1 Cycle)" : "Realistic (2 Cycle)")}",
                    $"Real-Time UI:   {(c.ShowRealTimeMetrics ? "Enabled" : "Disabled")}",
                    "Back"
                };

                selected = MenuHandler.Show("Configuration", "Modify simulation parameters", options, selected);

                if (selected == -1 || selected == 8) { loader.SaveConfig(); break; }
                
                if (selected == 0) c.Architecture = c.Architecture == "rv64" ? "rv32" : "rv64";
                if (selected == 1) c.ExecutionMode = c.ExecutionMode == "pipeline" ? "simple" : "pipeline";
                if (selected == 2) c.SteppingMode = c.SteppingMode == "auto" ? "manual" : "auto";
                if (selected == 3) c.LogLevel = c.LogLevel == SimulationLogLevel.None ? SimulationLogLevel.Simple : (c.LogLevel == SimulationLogLevel.Simple ? SimulationLogLevel.Verbose : SimulationLogLevel.None);
                if (selected == 4)
                {
                    c.BranchPredictor = c.BranchPredictor.ToLowerInvariant() switch {
                        "static" => "bimodal-1bit", "bimodal-1bit" => "bimodal-2bit", "bimodal-2bit" => "bimodal-3bit",
                        "bimodal-3bit" => "gshare", "gshare" => "static", _ => "static"
                    };
                }
                if (selected == 5) { c.PredictorInitValue++; if (c.PredictorInitValue > 3) c.PredictorInitValue = 0; }
                if (selected == 6) c.EnableEarlyBranchResolution = !c.EnableEarlyBranchResolution;
                if (selected == 7) c.ShowRealTimeMetrics = !c.ShowRealTimeMetrics;
            }
        }

        static void ShowProgramList(ProgramLoader loader)
        {
            var files = loader.GetAvailablePrograms();
            if (files.Length == 0) { Console.WriteLine("No programs found."); Console.ReadKey(); return; }
            
            var names = files.Select(Path.GetFileName).Concat(new[] { "Back" }).ToArray();
            
            int selected = MenuHandler.Show("Select Program", "", names!);
            if (selected == -1 || selected == names.Length - 1) return;

            RunSimulation(loader, files[selected]);
        }

        static void RunSimulation(ProgramLoader loader, string file)
        {
            using var session = loader.PrepareSession(file);
            bool isManual = loader.Config.SteppingMode == "manual";
            bool isPipeline = loader.Config.ExecutionMode == "pipeline";
            bool showRt = loader.Config.ShowRealTimeMetrics;
            
            _consoleScroll = 0;
            int cycles = 0;
            Stopwatch perfTimer = new Stopwatch(); 
            
            Console.Clear(); 
            RenderUI(session, cycles, isPipeline, perfTimer.Elapsed, showRt);

            while (!session.State.Halted)
            {
                if (cycles > loader.Config.MaxCycles) break;

                if (isManual)
                {
                    perfTimer.Stop(); 
                    var k = Console.ReadKey(true).Key;
                    
                    if (k == ConsoleKey.UpArrow) { _consoleScroll++; RenderUI(session, cycles, isPipeline, perfTimer.Elapsed, showRt); }
                    else if (k == ConsoleKey.DownArrow) { _consoleScroll = Math.Max(0, _consoleScroll - 1); RenderUI(session, cycles, isPipeline, perfTimer.Elapsed, showRt); }
                    else if (k == ConsoleKey.End) { _consoleScroll = 0; RenderUI(session, cycles, isPipeline, perfTimer.Elapsed, showRt); }
                    else if (k == ConsoleKey.Q) { session.State.Halted = true; break; }
                    else if (k == ConsoleKey.R) { isManual = false; continue; } 
                    else if (k == ConsoleKey.Enter || k == ConsoleKey.Spacebar)
                    {
                        perfTimer.Start();
                        if (isPipeline) session.PipelinedRunner?.Step(cycles);
                        else session.SimpleRunner?.Run(1);
                        cycles++;
                        perfTimer.Stop();
                        RenderUI(session, cycles, isPipeline, perfTimer.Elapsed, showRt);
                    }
                }
                else
                {
                    perfTimer.Start();
                    if (isPipeline) session.PipelinedRunner?.Step(cycles);
                    else session.SimpleRunner?.Run(1);
                    cycles++;
                    
                    if (cycles % 2000 == 0)
                    {
                        perfTimer.Stop(); 
                        if (Console.KeyAvailable)
                        {
                            RenderUI(session, cycles, isPipeline, perfTimer.Elapsed, showRt);
                            Console.ReadKey(true); 
                            
                            Console.SetCursorPosition(0, _windowHeight - 2);
                            Console.BackgroundColor = ConsoleColor.DarkYellow;
                            Console.ForegroundColor = ConsoleColor.Black;
                            Console.Write(" [PAUSED] Manual Mode. Enter: Step | 'R': Resume ");
                            Console.ResetColor();
                            
                            var nextKey = Console.ReadKey(true).Key;
                            if (nextKey == ConsoleKey.R) { /* Resume */ }
                            else if (nextKey == ConsoleKey.Q) { session.State.Halted = true; }
                            else { isManual = true; } 
                        }
                        else
                        {
                             RenderUI(session, cycles, isPipeline, perfTimer.Elapsed, showRt);
                        }
                        perfTimer.Start();
                    }
                }
            }
            
            perfTimer.Stop();
            int endRow = RenderUI(session, cycles, isPipeline, perfTimer.Elapsed, showRt);
            DrawReport(session, cycles, perfTimer.Elapsed, endRow);
            Console.ReadKey(true);
        }

        static int RenderUI(SimulationSession s, int cycle, bool isPipeline, TimeSpan elapsed, bool showRt)
        {
            Console.SetCursorPosition(0, 0);
            
            WriteColorLine(ConsoleColor.Cyan, FormatLineTruncate($" CYCLE: {cycle,-10} | PC: 0x{s.State.Registers.PC:X8} | TIME: {elapsed.TotalSeconds:F4}s"));

            Console.WriteLine(FormatLineTruncate(" [Registers]"));
            for(int i=0; i<32; i+=4)
            {
                string r1 = $"x{i}={s.State.Registers.Read(i):X8}";
                string r2 = $"x{i+1}={s.State.Registers.Read(i+1):X8}";
                string r3 = $"x{i+2}={s.State.Registers.Read(i+2):X8}";
                string r4 = $"x{i+3}={s.State.Registers.Read(i+3):X8}";
                Console.WriteLine(FormatLineTruncate($" {r1,-15} {r2,-15} {r3,-15} {r4,-15}"));
            }

            int pipeStartRow = Console.CursorTop;

            if (showRt && isPipeline && s.PipelinedRunner?.Metrics != null)
            {
                DrawSidePanel(s.PipelinedRunner.Metrics, 85, 2);
            }

            // --- UPDATED PIPELINE VISUALIZATION (Added @[PC]) ---
            Console.WriteLine(FormatLineTruncate(" [Pipeline Stages]"));
            if (isPipeline && s.PipelinedRunner != null)
            {
                var pipe = s.PipelinedRunner.PipelineState;
                
                // IF
                Console.WriteLine(FormatLineTruncate($" IF:  [{FormatHex(pipe.FetchDecode.Instruction)}]@[{pipe.FetchDecode.PC:X8}] Stall:{pipe.FetchDecode.IsStalled} Pred:{pipe.FetchDecode.PredictedTaken}"));
                
                // ID
                Console.WriteLine(FormatLineTruncate($" ID:  [{FormatHex(pipe.DecodeExecute.RawInstruction)}]@[{pipe.DecodeExecute.PC:X8}] {FormatMnemonic(pipe.DecodeExecute.DecodedInst?.Mnemonic)}"));
                
                // EX
                Console.WriteLine(FormatLineTruncate($" EX:  [{FormatHex(pipe.ExecuteMemory.RawInstruction)}]@[{pipe.ExecuteMemory.PC:X8}] {FormatMnemonic(pipe.ExecuteMemory.DecodedInst?.Mnemonic)} Mis:{pipe.ExecuteMemory.Misprediction}"));
                
                // MEM
                Console.WriteLine(FormatLineTruncate($" MEM: [{FormatHex(pipe.MemoryWriteback.RawInstruction)}]@[{pipe.MemoryWriteback.PC:X8}] {FormatMnemonic(pipe.MemoryWriteback.DecodedInst?.Mnemonic)}"));
                
                // WB (Updated to match other stages + RegWrite info)
                Console.WriteLine(FormatLineTruncate($" WB:  [{FormatHex(pipe.MemoryWriteback.RawInstruction)}]@[{pipe.MemoryWriteback.PC:X8}] RegWrite:{pipe.MemoryWriteback.RegWrite} Rd:{pipe.MemoryWriteback.Rd} Val:{pipe.MemoryWriteback.FinalResult:X}"));
            }
            else
            {
                for(int i=0; i<5; i++) Console.WriteLine(FormatLineTruncate(""));
            }
            // ----------------------------------------------------

            Console.WriteLine(FormatLineTruncate($" [Console Output] (Scroll: {_consoleScroll})"));
            Console.BackgroundColor = ConsoleColor.DarkBlue;
            Console.ForegroundColor = ConsoleColor.White;
            
            var consoleLines = GetWrappedLines(s.OutputBuffer.ToString(), Console.WindowWidth - 2);
            int totalLines = consoleLines.Count;
            int viewportHeight = 8;
            
            int endLine = totalLines - _consoleScroll;
            if (endLine > totalLines) endLine = totalLines;
            
            int startLine = endLine - viewportHeight;
            if (startLine < 0) startLine = 0;

            for(int i = 0; i < viewportHeight; i++) 
            {
                int lineIdx = startLine + i;
                string content = (lineIdx < endLine && lineIdx < totalLines) ? consoleLines[lineIdx] : "";
                Console.WriteLine(FormatLineTruncate(" " + content));
            }
            Console.ResetColor();

            Console.WriteLine(FormatLineTruncate(" [Controls] Arrows: Scroll | Enter: Step | R: Run All | Q: Quit"));
            
            return Console.CursorTop; 
        }

        static void DrawReport(SimulationSession s, int cycles, TimeSpan elapsed, int startRow)
        {
            Console.SetCursorPosition(0, startRow);
            for(int i=startRow; i<_windowHeight; i++) Console.WriteLine(FormatLineTruncate(""));
            
            Console.SetCursorPosition(0, startRow);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("========================================");
            Console.WriteLine(" FINAL PERFORMANCE REPORT");
            Console.WriteLine("========================================");
            Console.ResetColor();

            var metrics = s.PipelinedRunner?.Metrics;
            
            WriteMetric("Total Cycles", cycles, ConsoleColor.White);
            WriteMetric("Instructions", metrics?.InstructionsRetired ?? 0, ConsoleColor.White);
            WriteMetric("IPC", metrics?.IPC ?? 0, ConsoleColor.Cyan, "F2", " (Instr/Cycle)");
            WriteMetric("CPI", metrics?.CPI ?? 0, ConsoleColor.Cyan, "F2", " (Cycles/Instr)");

            Console.WriteLine("----------------------------------------");
            
            if (metrics != null)
            {
                var accColor = metrics.BranchAccuracy > 90 ? ConsoleColor.Green : (metrics.BranchAccuracy > 70 ? ConsoleColor.Yellow : ConsoleColor.Red);
                WriteMetric("Accuracy", metrics.BranchAccuracy, accColor, "F2", "%");

                Console.Write($" {"Misses",-16} ");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write($"{metrics.BranchMisses:N0}");
                Console.ResetColor();
                Console.Write(" / ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"{metrics.TotalBranches:N0}");
                
                double flushPerc = (double)metrics.ControlHazardFlushes / Math.Max(1, metrics.TotalCycles) * 100.0;
                WriteMetric("Flushes", metrics.ControlHazardFlushes, ConsoleColor.Yellow, "N0", $" ({flushPerc:F1}%)");
                
                double stallPerc = (double)metrics.DataHazardStalls / Math.Max(1, metrics.TotalCycles) * 100.0;
                WriteMetric("Stalls", metrics.DataHazardStalls, ConsoleColor.Yellow, "N0", $" ({stallPerc:F1}%)");
            }

            Console.WriteLine("----------------------------------------");
            double time = elapsed.TotalSeconds;
            double khz = (time > 0) ? (cycles / time) / 1000.0 : 0;
            double mips = (time > 0 && metrics != null) ? (metrics.InstructionsRetired / time) / 1000000.0 : 0;
            
            WriteMetric("Sim Time", time, ConsoleColor.Cyan, "F4", "s");
            WriteMetric("Frequency", khz, ConsoleColor.Cyan, "F2", " kHz");
            WriteMetric("Speed", mips, ConsoleColor.Cyan, "F2", " MIPS");
            
            Console.WriteLine("\nPress any key to return to menu...");
        }

        static void WriteMetric(string label, object value, ConsoleColor valColor, string format = "N0", string suffix = "")
        {
            Console.Write($" {label,-16} "); 
            Console.ForegroundColor = valColor;
            Console.Write(string.Format("{0:" + format + "}", value));
            Console.ResetColor();
            Console.WriteLine(suffix);
        }

        static void WriteColorLine(ConsoleColor color, string text)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(text);
            Console.ResetColor();
        }

        static void DrawSidePanel(PerformanceMetrics m, int leftX, int startY)
        {
            void PrintAt(int y, string label, object val, ConsoleColor color, string fmt = "N0", string sfx = "")
            {
                Console.SetCursorPosition(leftX, y);
                Console.Write($"{label}: ");
                Console.ForegroundColor = color;
                Console.Write(string.Format("{0:" + fmt + "}", val));
                Console.ResetColor();
                Console.Write(sfx);
            }

            PrintAt(startY + 0, "REAL-TIME METRICS", "", ConsoleColor.White);
            PrintAt(startY + 1, "------------------", "", ConsoleColor.Gray);
            PrintAt(startY + 2, "IPC       ", m.IPC, ConsoleColor.Cyan, "F2");
            PrintAt(startY + 3, "Retired   ", m.InstructionsRetired, ConsoleColor.White);
            
            var accColor = m.BranchAccuracy > 90 ? ConsoleColor.Green : (m.BranchAccuracy > 70 ? ConsoleColor.Yellow : ConsoleColor.Red);
            PrintAt(startY + 5, "Accuracy  ", m.BranchAccuracy, accColor, "F2", "%");
            PrintAt(startY + 6, "Br. Hits  ", m.BranchHits, ConsoleColor.Green);
            PrintAt(startY + 7, "Br. Miss  ", m.BranchMisses, ConsoleColor.Red);
            PrintAt(startY + 9, "Flushes   ", m.ControlHazardFlushes, ConsoleColor.Yellow);
            PrintAt(startY + 10,"Stalls    ", m.DataHazardStalls, ConsoleColor.Yellow);
        }

        static class MenuHandler
        {
            public static int Show(string title, string subtitle, string[] options, int selected = 0)
            {
                Console.Clear(); 
                
                while (true)
                {
                    Console.SetCursorPosition(0, 0);
                    
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(FormatLineTruncate("========================================"));
                    Console.WriteLine(FormatLineTruncate($"       {title}")); 
                    Console.WriteLine(FormatLineTruncate("========================================"));
                    Console.ResetColor();
                    
                    if(!string.IsNullOrEmpty(subtitle)) Console.WriteLine(FormatLineTruncate($" {subtitle}"));
                    Console.WriteLine(FormatLineTruncate("")); 
                    
                    for (int i = 0; i < options.Length; i++)
                    {
                        if (i == selected)
                        {
                            Console.BackgroundColor = ConsoleColor.Gray;
                            Console.ForegroundColor = ConsoleColor.Black;
                            Console.WriteLine(FormatLineTruncate($" > {options[i].PadRight(30)} "));
                            Console.ResetColor();
                        }
                        else
                        {
                            Console.WriteLine(FormatLineTruncate($"   {options[i]}"));
                        }
                    }
                    Console.WriteLine(FormatLineTruncate("")); 
                    Console.WriteLine(FormatLineTruncate(" [Arrows: Move | Enter: Select]"));

                    var key = Console.ReadKey(true).Key;
                    if (key == ConsoleKey.UpArrow) { selected--; if (selected < 0) selected = options.Length - 1; }
                    else if (key == ConsoleKey.DownArrow) { selected++; if (selected >= options.Length) selected = 0; }
                    else if (key == ConsoleKey.Enter || key == ConsoleKey.Spacebar) return selected;
                    else if (key == ConsoleKey.Escape) return -1;
                }
            }
        }

        static List<string> GetWrappedLines(string fullText, int width)
        {
            var result = new List<string>();
            var logicalLines = fullText.Replace("\r\n", "\n").Split('\n');
            foreach (var line in logicalLines) {
                if (string.IsNullOrEmpty(line)) { result.Add(""); continue; }
                for (int i = 0; i < line.Length; i += width) {
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
