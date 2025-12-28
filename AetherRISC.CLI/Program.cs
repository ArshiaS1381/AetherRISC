using System;
using System.IO;
using System.Text;
using System.Threading;
using AetherRISC.Core.Architecture.Simulation.Runners;

namespace AetherRISC.CLI
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "AetherRISC CLI";
            Console.OutputEncoding = Encoding.UTF8;
            ProgramLoader loader;

            try { loader = new ProgramLoader("config.json"); }
            catch (Exception ex) { Console.WriteLine($"Startup Error: {ex.Message}"); return; }

            while (true)
            {
                Console.Clear();
                DrawHeader();
                Console.WriteLine($" Current Config: [{loader.Config.Architecture.ToUpper()}] [{loader.Config.ExecutionMode.ToUpper()}] [{loader.Config.SteppingMode.ToUpper()}]");
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
            Console.WriteLine("       AetherRISC Simulator v2.1");
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
                Console.WriteLine(" Settings (Changes saved immediately)");
                Console.WriteLine("----------------------------------------");
                Console.WriteLine($"  [1] Architecture:   {c.Architecture.ToUpper()}");
                Console.WriteLine($"  [2] Execution Mode: {c.ExecutionMode.ToUpper()}");
                Console.WriteLine($"  [3] Stepping Mode:  {c.SteppingMode.ToUpper()}");
                Console.WriteLine("  [B] Back");
                Console.WriteLine("----------------------------------------");
                Console.Write(" Toggle > ");

                var key = Console.ReadKey(true).Key;
                if (key == ConsoleKey.B) break;
                
                if (key == ConsoleKey.D1) c.Architecture = c.Architecture == "rv64" ? "rv32" : "rv64";
                if (key == ConsoleKey.D2) c.ExecutionMode = c.ExecutionMode == "pipeline" ? "simple" : "pipeline";
                if (key == ConsoleKey.D3) c.SteppingMode = c.SteppingMode == "auto" ? "manual" : "auto";
                
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
            
            while (!session.State.Halted)
            {
                if (isManual)
                {
                    RenderUI(session, cycles, isPipeline);
                    var k = Console.ReadKey(true).Key;
                    if (k == ConsoleKey.Q) { session.State.Halted = true; break; }
                    if (k == ConsoleKey.R) isManual = false; // Switch to Auto
                }

                if (isPipeline) session.PipelinedRunner?.Step(cycles);
                else 
                {
                    // Simple Runner handles its own loop usually, but for stepping we assume single step ability
                    // However, SimpleRunner.Run is a loop. We need to instantiate a new decoder or add Step to SimpleRunner.
                    // For now, to support SimpleRunner Stepping, we would need to modify SimpleRunner.
                    // Assuming SimpleRunner runs fully in Auto, or we just call Run(1).
                    // Actually, SimpleRunner.Run takes maxCycles. We call Run(1) inside the loop.
                     session.SimpleRunner?.Run(1);
                }

                cycles++;
                if (cycles > loader.Config.MaxCycles) break;
                
                if (!isManual) 
                {
                    // In Auto mode, print progress sparingly or simple status
                    if (cycles % 1000 == 0) { Console.Write("."); }
                }
            }
            
            RenderUI(session, cycles, isPipeline); // Final state
            Console.WriteLine("\nExecution Halted.");
        }

        static void RenderUI(SimulationSession s, int cycle, bool isPipeline)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($" CYCLE: {cycle} | PC: 0x{s.State.Registers.PC:X} | STATUS: {(s.State.Halted ? "HALTED" : "RUNNING")}");
            Console.ResetColor();
            
            // Draw Registers (Compact)
            Console.WriteLine("\n [Registers]");
            for(int i=0; i<32; i+=4)
            {
                Console.Write($" x{i,-2}={s.State.Registers.Read(i):X8}  ");
                Console.Write($" x{i+1,-2}={s.State.Registers.Read(i+1):X8}  ");
                Console.Write($" x{i+2,-2}={s.State.Registers.Read(i+2):X8}  ");
                Console.WriteLine($" x{i+3,-2}={s.State.Registers.Read(i+3):X8}");
            }

            // Draw Pipeline if active
            if (isPipeline && s.PipelinedRunner != null)
            {
                var pipe = s.PipelinedRunner.PipelineState;
                Console.WriteLine("\n [Pipeline Stages]");
                Console.WriteLine($" IF:  [{FormatHex(pipe.FetchDecode.Instruction)}] Stall:{pipe.FetchDecode.IsStalled}");
                Console.WriteLine($" ID:  [{FormatHex(pipe.DecodeExecute.RawInstruction)}] {pipe.DecodeExecute.DecodedInst?.Mnemonic ?? "-"}");
                Console.WriteLine($" EX:  [{FormatHex(pipe.ExecuteMemory.RawInstruction)}] {pipe.ExecuteMemory.DecodedInst?.Mnemonic ?? "-"}");
                Console.WriteLine($" MEM: [{FormatHex(pipe.MemoryWriteback.RawInstruction)}] {pipe.MemoryWriteback.DecodedInst?.Mnemonic ?? "-"}");
                Console.WriteLine($" WB:  RegWrite:{pipe.MemoryWriteback.RegWrite} Rd:{pipe.MemoryWriteback.Rd} Val:{pipe.MemoryWriteback.FinalResult:X}");
            }

            // Draw ECALL Output
            Console.WriteLine("\n [Console Output]");
            Console.BackgroundColor = ConsoleColor.DarkBlue;
            Console.ForegroundColor = ConsoleColor.White;
            string output = s.OutputBuffer.ToString();
            // Show last 5 lines
            var lines = output.Split('\n');
            int start = Math.Max(0, lines.Length - 5);
            for(int i=start; i<lines.Length; i++) 
                Console.WriteLine(" " + lines[i].TrimEnd().PadRight(Console.WindowWidth - 2));
            Console.ResetColor();

            Console.WriteLine("\n [Controls] Enter: Step | R: Run All | Q: Quit");
        }

        static string FormatHex(uint val) => val == 0 ? "        " : $"{val:X8}";
    }
}
