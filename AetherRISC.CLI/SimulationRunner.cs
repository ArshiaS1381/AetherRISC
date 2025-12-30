using System;
using System.Diagnostics;
using System.IO;
using AetherRISC.Core.Helpers;
using AetherRISC.CLI;

namespace AetherRISC.CLI
{
    public static class SimulationRunner
    {
        public static void Run(ProgramLoader loader, string file)
        {
            SimpleProfiler.Enabled = loader.Config.EnableProfiler;

            using var session = loader.PrepareSession(file);
            bool isManual = loader.Config.SteppingMode == "manual";
            bool isPipe = loader.Config.ExecutionMode == "pipeline";
            int maxCyc = loader.Config.MaxCycles;
            
            UiRenderer.ConsoleScroll = 0;
            int cycles = 0;
            var sw = new Stopwatch();

            Console.Clear();
            UiRenderer.Render(session, cycles, isPipe, sw.Elapsed, loader.Config.ShowRealTimeMetrics, false);

            while (!session.State.Halted && cycles < maxCyc)
            {
                if (isManual)
                {
                    var k = Console.ReadKey(true).Key;
                    if (k == ConsoleKey.Q) break;
                    
                    // Fix: Clamp using MaxConsoleScroll exposed by Renderer
                    if (k == ConsoleKey.UpArrow) { 
                        UiRenderer.ConsoleScroll = Math.Min(UiRenderer.MaxConsoleScroll, UiRenderer.ConsoleScroll + 1); 
                        UiRenderer.Render(session, cycles, isPipe, sw.Elapsed, true, false); 
                    }
                    else if (k == ConsoleKey.DownArrow) { 
                        UiRenderer.ConsoleScroll = Math.Max(0, UiRenderer.ConsoleScroll - 1); 
                        UiRenderer.Render(session, cycles, isPipe, sw.Elapsed, true, false); 
                    }
                    else if (k == ConsoleKey.R) isManual = false;
                    else if (k == ConsoleKey.Enter || k == ConsoleKey.Spacebar) {
                        Step(session, isPipe, sw); cycles++;
                        UiRenderer.Render(session, cycles, isPipe, sw.Elapsed, true, false);
                    }
                }
                else
                {
                    long startRender = sw.ElapsedMilliseconds;
                    while (sw.ElapsedMilliseconds - startRender < 33 && !session.State.Halted && cycles < maxCyc) {
                        Step(session, isPipe, sw); cycles++;
                    }
                    
                    if (Console.KeyAvailable) {
                        var k = Console.ReadKey(true).Key;
                        if (k == ConsoleKey.Q) break;
                        else if (k == ConsoleKey.Spacebar) isManual = true; 
                    }
                    
                    UiRenderer.Render(session, cycles, isPipe, sw.Elapsed, loader.Config.ShowRealTimeMetrics, false);
                }
            }
            
            sw.Stop();

            if (loader.Config.EnableTandemVerification && session.ShadowRunner != null)
            {
                RunTandemCheck(session, maxCyc);
            }
            
            bool showReport = true;
            if (session.VerificationFailed) Console.Beep();

            while (true)
            {
                UiRenderer.Render(session, cycles, isPipe, sw.Elapsed, true, showReport);
                
                var k = Console.ReadKey(true).Key;
                if (k == ConsoleKey.Q || k == ConsoleKey.Enter || k == ConsoleKey.Escape) break;
                
                // Fix: Clamp scroll here too
                if (k == ConsoleKey.UpArrow) UiRenderer.ConsoleScroll = Math.Min(UiRenderer.MaxConsoleScroll, UiRenderer.ConsoleScroll + 1);
                else if (k == ConsoleKey.DownArrow) UiRenderer.ConsoleScroll = Math.Max(0, UiRenderer.ConsoleScroll - 1);
            }
            
            SimpleProfiler.Reset();
        }

        static void Step(SimulationSession s, bool pipe, Stopwatch sw) {
            sw.Start();
            if(pipe && s.PipelinedRunner != null) s.PipelinedRunner.Step(1);
            else s.SimpleRunner?.Run(1);
            sw.Stop();
        }

        static void RunTandemCheck(SimulationSession s, int maxCycles)
        {
            if (s.ShadowRunner == null || s.ShadowState == null) return;

            int shadowCycles = 0;
            while (!s.ShadowState.Halted && shadowCycles < maxCycles)
            {
                s.ShadowRunner.Run(1);
                shadowCycles++;
            }

            for (int r = 1; r < 32; r++)
            {
                ulong pipeReg = s.State.Registers.Read(r);
                ulong shadowReg = s.ShadowState.Registers.Read(r);

                if (pipeReg != shadowReg)
                {
                    Fail(s, "Register Mismatch", $"x{r}", $"{pipeReg:X}", $"{shadowReg:X}");
                    return;
                }
            }

            string pipeOut = s.OutputBuffer.ToString() ?? "";
            string shadowOut = s.ShadowOutputBuffer?.ToString() ?? "";

            if (pipeOut != shadowOut)
            {
                string diffPipe = pipeOut.Length > 20 ? "..." + pipeOut.Substring(pipeOut.Length - 20) : pipeOut;
                string diffShad = shadowOut.Length > 20 ? "..." + shadowOut.Substring(shadowOut.Length - 20) : shadowOut;
                
                Fail(s, "Console Mismatch", "Output Buffer", diffPipe, diffShad);
                return;
            }

            s.VerificationPassed = true;
        }

        static void Fail(SimulationSession s, string reason, string loc, string act, string exp)
        {
            s.VerificationFailed = true;
            s.FailureDetails = new MismatchInfo
            {
                Reason = reason,
                Location = loc,
                Actual = act,
                Expected = exp
            };
        }
    }
}
