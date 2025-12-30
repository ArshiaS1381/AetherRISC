using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq; 
using AetherRISC.Core.Architecture.Hardware.Pipeline;
using AetherRISC.Core.Helpers;

namespace AetherRISC.CLI
{
    public static class UiRenderer
    {
        private static readonly StringBuilder _sb = new StringBuilder(32768);
        public static int ConsoleScroll = 0;
        public static int MaxConsoleScroll = 0;
        private static int _lastHeight = 0;

        // Theme
        private const string C_Frame = "\u001b[38;5;239m"; private const string C_Title = "\u001b[38;5;39m";
        private const string C_Label = "\u001b[38;5;245m"; private const string C_Val = "\u001b[38;5;255m";
        private const string C_Dim = "\u001b[38;5;236m";   private const string C_Cyan = "\u001b[38;5;51m";
        private const string C_Green = "\u001b[38;5;46m";  private const string C_Red = "\u001b[38;5;196m";
        private const string C_Yel = "\u001b[38;5;226m";   private const string C_Reset = "\u001b[0m";

        public static int Render(SimulationSession s, int cycle, bool isPipeline, TimeSpan elapsed, bool showRt, bool showReport = false)
        {
            _sb.Clear();
            int width = Math.Min(Console.WindowWidth, 140);
            if (width < 80) width = 80;

            string status = s.VerificationFailed ? $"{C_Red}FAILED" : 
                           (s.VerificationPassed ? $"{C_Green}VERIFIED" : 
                           (s.State.Halted ? $"{C_Red}STOPPED" : $"{C_Green}RUNNING"));
            
            // Header
            DrawBoxTop(width-1, $"AetherRISC v5.0 {C_Frame}| {status} {C_Frame}| {s.ProgramName} {C_Frame}| Cycle: {C_Val}{cycle} {C_Frame}| Time: {C_Val}{elapsed.TotalSeconds:F4}s");

            // Registers & Live Metrics (Always Show for context)
            DrawMetricsPanel(width, s, cycle, elapsed, showRt, isPipeline);

            if (!showReport) 
            {
                // Live Pipeline View
                DrawBoxMid(width-1, "Pipeline Stages");
                DrawPipelineGrid(width, s, isPipeline);
                
                // Live Console
                DrawBoxMid(width-1, $"Console Output");
                DrawConsole(width, s);
            }
            else
            {
                // --- FULL REPORT MODE ---
                DrawBoxMid(width-1, "EXECUTION REPORT");
                
                // 1. Config
                DrawConfigBlock(width, s);
                _sb.Append($"{C_Frame}├" + new string('─', width - 3) + $"┤\n");

                // 2. Metrics
                DrawPerformanceDetails(width, s, cycle, elapsed);
                
                // 3. Profiler
                if (SimpleProfiler.Enabled) {
                    _sb.Append($"{C_Frame}├" + new string('─', width - 3) + $"┤\n");
                    DrawProfilerData(width);
                }

                // 4. Tandem
                if (s.VerificationFailed || s.VerificationPassed) {
                    string tTitle = s.VerificationFailed ? $"{C_Red}TANDEM FAIL" : $"{C_Green}TANDEM PASS";
                    _sb.Append($"{C_Frame}├─ {tTitle} " + $"{C_Frame}" + new string('─', width - StripAnsi(tTitle).Length - 6) + $"{C_Frame}┤\n");
                    DrawTandemReport(width, s);
                }
                
                // 5. Final Console Snapshot
                _sb.Append($"{C_Frame}├─ Console Output " + new string('─', width - 20) + $"┤\n");
                DrawConsole(width, s);
            }

            string nav = showReport ? "[Arrows] Scroll [Q] Quit" : "[Arrows] Scroll [Enter] Step [R] Run All [Q] Quit";
            DrawBoxBottom(width-1, nav);

            Console.SetCursorPosition(0, 0);
            Console.Write(_sb.ToString());
            
            int currentH = _sb.ToString().Split('\n').Length;
            if (currentH < _lastHeight) { for(int i=currentH; i < _lastHeight; i++) Console.WriteLine(new string(' ', width)); }
            _lastHeight = currentH;
            return currentH;
        }

        // --- PANELS ---

        private static void DrawMetricsPanel(int width, SimulationSession s, int cycle, TimeSpan elapsed, bool showRt, bool isPipe)
        {
            var regs = s.State.Registers;
            var m = s.PipelinedRunner?.Metrics;
            for (int r = 0; r < 8; r++) {
                 StringBuilder line = new StringBuilder();
                 line.Append($"{C_Frame}│ {C_Label}");
                 for (int c = 0; c < 4; c++) {
                    int ri = (r * 4) + c;
                    ulong val = regs.Read(ri);
                    string vStr = val == 0 ? $"{C_Dim}00000000" : $"{C_Val}{val:X8}";
                    line.Append($"x{ri,-2}:{vStr}  ");
                }
                int visLen = StripAnsi(line.ToString()).Length;
                int gap = 68 - visLen;
                if (gap > 0) line.Append(new string(' ', gap));

                // Always show metrics if pipeline mode
                if (isPipe && m != null) {
                    switch(r) {
                        case 0: line.Append(Metric("IPC", $"{m.EffectiveIPC:F2}")); break;
                        case 1: line.Append(Metric("Freq", $"{(cycle/Math.Max(0.001, elapsed.TotalSeconds))/1000.0:F1}k")); break;
                        case 2: line.Append(Metric("Util", $"{C_Yel}{m.SlotUtilization:F1}%")); break;
                        case 3: line.Append(Metric("Miss", $"{C_Red}{m.BranchMisses}")); break;
                        case 4: line.Append(Metric("Flush", $"{C_Red}{m.ControlHazardFlushes}")); break;
                        case 5: line.Append(Metric("Stall", $"{C_Yel}{m.DataHazardStalls}")); break;
                        case 6: line.Append(Metric("Acc", $"{m.BranchAccuracy:F1}%")); break;
                    }
                } 
                else if (r == 3) line.Append($"{C_Dim}(Simple Mode)");
                int rem = width - StripAnsi(line.ToString()).Length - 2; 
                _sb.Append(line.ToString() + new string(' ', Math.Max(0, rem)) + $"{C_Frame}│\n");
            }
        }

        private static void DrawConfigBlock(int width, SimulationSession s)
        {
            var m = s.PipelinedRunner?.Metrics;
            if(m == null) return;
            
            // Layout:  Key: Value   Key: Value
            void Pair(string k1, string v1, string k2, string v2) {
                string part1 = $"{C_Label}{k1}: {C_Val}{v1}";
                string part2 = $"{C_Label}{k2}: {C_Val}{v2}";
                string line = $"{part1,-40} {part2}"; // Pad first part
                int len = StripAnsi(line).Length; 
                // Manual pad calc because ANSI messes up Align
                _sb.Append($"{C_Frame}│ {part1}");
                int p1Len = k1.Length + v1.Length + 2; 
                _sb.Append(new string(' ', Math.Max(2, 40 - p1Len)));
                _sb.Append($"{part2}");
                int p2Len = k2.Length + v2.Length + 2;
                _sb.Append(new string(' ', Math.Max(0, width - (p1Len + Math.Max(2, 40-p1Len) + p2Len) - 4)));
                _sb.Append($"{C_Frame}│\n");
            }
            
            // Hardcoded mainly because retrieving ArchitectureSettings from Runner is hard without casting
            // We assume standard setup
            Pair("Pipeline Width", $"{m.PipelineWidth}", "Mode", "Superscalar");
        }

        private static void DrawPerformanceDetails(int width, SimulationSession s, int cyc, TimeSpan el)
        {
            var m = s.PipelinedRunner?.Metrics;
            if (m == null) return;

            void L(string k, string v) {
                string line = $"{C_Label}{k,-25} {C_Val}{v}";
                _sb.Append($"{C_Frame}│ {line}{new string(' ', Math.Max(0, width - StripAnsi(line).Length - 4))}{C_Frame}│\n");
            }

            ulong eff = m.IsaInstructionsRetired;
            ulong pipe = m.InstructionsRetired;
            double effIpc = m.EffectiveIPC;
            double pipeIpc = m.PipelineIPC;

            L("Total Cycles", $"{cyc:N0}");
            
            // FIX: Correct subtraction order. Eff (ISA) is higher than Pipe (Commits) if fusion happens.
            // e.g. 2 Inst -> 1 MacroOp. Eff=2, Pipe=1.
            if (eff > pipe) {
                ulong fused = eff - pipe;
                L("Instructions (ISA)", $"{C_Green}{eff:N0} {C_Dim}(Effective)");
                L("Instructions (Pipe)", $"{C_Cyan}{pipe:N0} {C_Dim}({fused:N0} ops fused)");
                L("IPC (Effective)", $"{C_Green}{effIpc:F2} {C_Dim}({pipeIpc:F2} raw)");
                L("CPI (Effective)", $"{C_Green}{(1.0/effIpc):F2} {C_Dim}({(1.0/pipeIpc):F2} raw)");
            } else {
                L("Instructions Retired", $"{pipe:N0}");
                L("IPC", $"{pipeIpc:F2}");
                L("CPI", $"{(1.0/pipeIpc):F2}");
            }

            L("Execution Time", $"{el.TotalSeconds:F4}s");
            L("Simulation Speed", $"{(eff / Math.Max(0.0001, el.TotalSeconds)) / 1e6:F2} MIPS");
            L("Branch Accuracy", $"{m.BranchAccuracy:F2}% ({m.BranchMisses} misses)");
            L("Data Hazard Stalls", $"{m.DataHazardStalls}");
            L("Control Flushes", $"{m.ControlHazardFlushes}");
        }

        private static void DrawProfilerData(int width)
        {
            string pd = SimpleProfiler.Dump();
            if(pd.Contains("No data") || pd.Contains("Disabled")) return;

            foreach(var pl in pd.Split('\n', StringSplitOptions.RemoveEmptyEntries)) {
                if(pl.Contains("===")) continue;
                string line = pl.Trim().Replace(":", $"{C_Label}:").Replace("|", $"{C_Frame}|{C_Val}");
                _sb.Append($"{C_Frame}│ {C_Yel}{line}{new string(' ', Math.Max(0, width - StripAnsi(line).Length - 4))}{C_Frame}│\n");
            }
        }

        private static void DrawTandemReport(int width, SimulationSession s)
        {
            if (s.VerificationFailed && s.FailureDetails != null) {
                var f = s.FailureDetails;
                void L(string k, string v, string col) {
                    string c = $"{C_Label}{k,-12} {col}{v}";
                    _sb.Append($"{C_Frame}│ {c}{new string(' ', Math.Max(0, width - StripAnsi(c).Length - 4))}{C_Frame}│\n");
                }
                L("Reason:", f.Reason, C_Red); 
                L("Location:", f.Location, C_Val); 
                L("Pipeline:", f.Actual, C_Red); 
                L("Golden:", f.Expected, C_Green);
                
                // Show console diff only on fail
                _sb.Append($"{C_Frame}├" + new string('─', width - 2) + $"┤\n");
                string pOut = (s.OutputBuffer as RingBufferWriter)?.ToString() ?? "";
                string sOut = (s.ShadowOutputBuffer as RingBufferWriter)?.ToString() ?? "";
                string pL = pOut.Split('\n', StringSplitOptions.RemoveEmptyEntries).LastOrDefault() ?? "<empty>";
                string sL = sOut.Split('\n', StringSplitOptions.RemoveEmptyEntries).LastOrDefault() ?? "<empty>";
                
                if(pL.Length > 40) pL = "..." + pL.Substring(pL.Length - 40);
                if(sL.Length > 40) sL = "..." + sL.Substring(sL.Length - 40);

                L("Console(P):", pL, C_Dim);
                L("Console(G):", sL, C_Dim);
            } 
            else {
                string msg = $"{C_Green}  ALL CHECKS PASSED: Pipeline state and output match Golden Model.";
                _sb.Append($"{C_Frame}│{msg}" + new string(' ', Math.Max(0, width - StripAnsi(msg).Length - 3)) + $"{C_Frame}│\n");
            }
        }

        private static void DrawPipelineGrid(int width, SimulationSession s, bool isPipeline)
        {
            if (isPipeline && s.PipelinedRunner != null) {
                var p = s.PipelinedRunner.PipelineState;
                RenderStageGrid(width, "IF ", p.FetchDecode,     op => $"{C_Val}{op.RawInstruction:X8} {C_Label}@{op.PC:X4}");
                RenderStageGrid(width, "ID ", p.DecodeExecute,   op => Disassemble(op));
                RenderStageGrid(width, "EX ", p.ExecuteMemory,   op => Disassemble(op));
                RenderStageGrid(width, "MEM", p.MemoryWriteback, op => Disassemble(op));
                RenderStageGrid(width, "WB ", p.MemoryWriteback, op => op.RegWrite ? $"x{op.Rd}={op.FinalResult:X}" : "-");
            } else {
                string txt = "   Scalar mode. Pipeline view disabled.";
                _sb.Append($"{C_Frame}│{C_Dim}{txt}{new string(' ', width - txt.Length - 4)}{C_Frame}│\n");
            }
        }

        private static void RenderStageGrid(int width, string name, PipelineStageBuffer buf, Func<PipelineMicroOp, string> fmt)
        {
            int slots = buf.Slots.Length;
            int cols = (slots >= 8) ? 4 : (slots >= 4 ? 2 : 1);
            int rows = (int)Math.Ceiling((double)slots / cols);
            string stCol = buf.IsStalled ? C_Red : C_Cyan;
            string stTxt = buf.IsStalled ? "STALL" : name;
            for (int r = 0; r < rows; r++) {
                StringBuilder line = new StringBuilder();
                line.Append($"{C_Frame}│ {stCol}{stTxt,-5}{C_Frame}│ ");
                int cellW = (width - 12) / cols;
                for (int c = 0; c < cols; c++) {
                    int i = r * cols + c;
                    if (i < slots) {
                        var s = buf.Slots[i];
                        string cell = $"{C_Dim}{i,2} {(s.Valid ? fmt(s) : ".")}";
                        // Strict Clipping
                        int strip = StripAnsi(cell).Length;
                        if(strip > cellW - 1) { 
                            // Re-build string if too long (hard to substring ANSI)
                            cell = $"{C_Dim}{i,2} .."; 
                            strip = 5;
                        }
                        line.Append(cell);
                        line.Append(new string(' ', Math.Max(0, cellW - strip)));
                    }
                }
                int rem = width - StripAnsi(line.ToString()).Length - 2;
                _sb.Append(line.ToString() + new string(' ', Math.Max(0, rem)) + $"{C_Frame}│\n");
                stTxt = "";
            }
        }

        private static void DrawConsole(int width, SimulationSession s)
        {
            var rawLines = (s.OutputBuffer is RingBufferWriter rb) ? rb.Snapshot() : new List<string>();
            var wrapped = new List<string>();
            int maxW = width - 4;
            foreach (var line in rawLines) {
                string rem = line ?? "";
                if (rem == "") wrapped.Add("");
                while (rem.Length > maxW) { wrapped.Add(rem.Substring(0, maxW)); rem = rem.Substring(maxW); }
                if (rem.Length > 0) wrapped.Add(rem);
            }
            MaxConsoleScroll = Math.Max(0, wrapped.Count - 6);
            ConsoleScroll = Math.Clamp(ConsoleScroll, 0, MaxConsoleScroll);
            int start = Math.Max(0, wrapped.Count - 6 - ConsoleScroll);
            for (int i = 0; i < 6; i++) {
                string t = (start + i < wrapped.Count) ? wrapped[start + i] : "";
                _sb.Append($"{C_Frame}│ {C_Val}{t}{new string(' ', Math.Max(0, width - t.Length - 4))}{C_Frame}│\n");
            }
        }

        private static void DrawBoxTop(int w, string t) => _sb.Append($"{C_Frame}┌─ {C_Title}{t} {C_Frame}{new string('─', Math.Max(0, w - StripAnsi(t).Length - 5))}┐\n");
        private static void DrawBoxMid(int w, string t) => _sb.Append($"{C_Frame}├─ {C_Title}{t} {C_Frame}{new string('─', Math.Max(0, w - StripAnsi(t).Length - 5))}┤\n");
        private static void DrawBoxBottom(int w, string t) => _sb.Append($"{C_Frame}└─ {C_Label}{t} {C_Frame}{new string('─', Math.Max(0, w - StripAnsi(t).Length - 5))}┘{C_Reset}\n");
        private static string StripAnsi(string s) => Regex.Replace(s, "\u001b\\[[0-9;]*m", "");
        private static string Metric(string l, string v) => $"{C_Cyan}{l,-6} {C_Val}{v}";
        private static string Disassemble(PipelineMicroOp op) => op.DecodedInst?.Mnemonic ?? "-";
    }
}
