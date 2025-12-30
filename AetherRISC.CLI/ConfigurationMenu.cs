using System;
using System.Text;
using System.Text.RegularExpressions;

namespace AetherRISC.CLI
{
    public static class ConfigurationMenu
    {
        public static void Show(ProgramLoader loader)
        {
            int sel = 1;
            Console.Clear();

            string C_Frame = "\u001b[38;5;239m"; string C_Title = "\u001b[38;5;39m";
            string C_White = "\u001b[38;5;255m"; string C_Gray = "\u001b[38;5;245m";
            string C_SelBg = "\u001b[48;5;237m"; string C_SelFg = "\u001b[38;5;255m";
            string C_Green = "\u001b[38;5;46m";  string C_Red = "\u001b[38;5;196m";
            string C_Cyan  = "\u001b[38;5;51m";  string C_Yel = "\u001b[38;5;226m";
            string C_Reset = "\u001b[0m"; string C_Shadow = "\u001b[48;5;232m ";
            string C_Dim   = "\u001b[38;5;240m";

            while(true)
            {
                var c = loader.Config;
                string OnOff(bool v) => v ? $"{C_Green}ENABLED" : $"{C_Red}DISABLED";
                string WCol = c.PipelineWidth == 1 ? C_Gray : (c.PipelineWidth > 8 ? C_Red : C_Title);
                var (pMax, pDesc) = GetPredInfo(c.BranchPredictor, c.PredictorInitValue);
                
                // Logic: Early Resolution (True) = 1 Cycle Penalty. Late (False) = 2 Cycles.
                string penalty = c.EnableEarlyBranchResolution ? $"{C_Green}1 Cycle (Ideal)" : $"{C_Yel}2 Cycles (Realistic)";

                string[] opts = new[] {
                    $"Instruction Set {C_Yel}CONF >", 
                    $"Architecture    {C_White}{(c.Architecture == "rv64" ? "RV64" : "RV32")}",
                    $"Execution Mode  {C_White}{c.ExecutionMode.ToUpper()}",
                    $"Tandem Verify   {OnOff(c.EnableTandemVerification)}", 
                    "--- PIPELINE ---",
                    $"Pipeline Width  {WCol}{c.PipelineWidth,-2}", 
                    $"Fetch Ratio     {C_Cyan}{c.FetchBufferRatio:F1}x", 
                    $"Flush Penalty   {penalty}", // New
                    $"Dynamic Fetch   {OnOff(c.AllowDynamicBranchFetching)}",
                    $"Cascaded Exec   {OnOff(c.AllowCascadedExecution)}", 
                    $"Macro-Op Fusion {OnOff(c.EnableMacroOpFusion)}",
                    $"RAS Prediction  {OnOff(c.EnableReturnAddressStack)}",
                    "--- SIMULATION ---",
                    $"Max Cycles      {C_White}{c.MaxCycles:N0}",
                    $"Profiler        {OnOff(c.EnableProfiler)}",
                    $"Stepping Mode   {C_White}{c.SteppingMode.ToUpper()}",
                    $"Branch Pred     {C_Title}{c.BranchPredictor.ToUpper()}",
                    $"Pred Init Val   {C_White}{c.PredictorInitValue} {C_Gray}({pDesc})",
                    $"Show Metrics    {OnOff(c.ShowRealTimeMetrics)}",
                    $"{C_Yel}Save & Return"
                };

                Console.SetCursorPosition(0,0);
                int w = 60;
                int left = Math.Max(0, (Console.WindowWidth - w) / 2);
                string padL = new string(' ', left);
                
                StringBuilder sb = new StringBuilder();
                sb.Append("\n\n");
                sb.Append($"{padL}{C_Frame}┌─ {C_Title}SETTINGS {C_Frame}" + new string('─', w - 13) + "┐\n");

                for(int i=0; i<opts.Length; i++) {
                    sb.Append($"{padL}{C_Frame}│ ");
                    string label = opts[i];
                    bool isSep = label.StartsWith("---");
                    
                    if (isSep) {
                        string txt = label.Replace("-", "").Trim();
                        int available = w - 4; 
                        int txtLen = txt.Length;
                        int dashLen = (available - txtLen - 2) / 2;
                        string dash = new string('─', dashLen);
                        string line = $"{C_Dim}{dash} {C_White}{txt} {C_Dim}{dash}";
                        if ((dashLen * 2) + txtLen + 2 < available) line += "─";
                        sb.Append($"{line} ");
                    }
                    else {
                        int visLen = Regex.Replace(label, "\u001b\\[[0-9;]*m", "").Length;
                        int pad = Math.Max(0, w - 7 - visLen);
                        
                        if (i==sel) sb.Append($"{C_SelBg}{C_SelFg} > {label}{new string(' ', pad)} {C_Reset}");
                        else sb.Append($"   {label}{new string(' ', pad)} ");
                    }
                    sb.Append($"{C_Frame}│{C_Shadow}{C_Reset}\n");
                }
                
                sb.Append($"{padL}{C_Frame}└" + new string('─', w - 2) + $"┘{C_Shadow}{C_Reset}\n");
                sb.Append(new string(' ', Console.WindowWidth) + "\n");
                Console.Write(sb.ToString());

                var key = Console.ReadKey(true).Key;

                if (key == ConsoleKey.UpArrow) { 
                    do { sel--; if (sel < 0) sel = opts.Length-1; } while(opts[sel].StartsWith("---"));
                }
                else if (key == ConsoleKey.DownArrow) {
                    do { sel++; if (sel >= opts.Length) sel = 0; } while(opts[sel].StartsWith("---"));
                }
                else if (key == ConsoleKey.Enter || key == ConsoleKey.Escape) {
                    if (sel == 0) InstructionConfigMenu.Show(loader.Config); 
                    else if (sel == opts.Length - 1 || key == ConsoleKey.Escape) { 
                        Console.Clear(); loader.SaveConfig(); break; 
                    }
                    else if (key == ConsoleKey.Enter) HandleEdit(sel, 1, c);
                }
                else if (key == ConsoleKey.LeftArrow || key == ConsoleKey.RightArrow) {
                    int dir = (key == ConsoleKey.RightArrow) ? 1 : -1;
                    HandleEdit(sel, dir, c);
                }
            }
        }

        private static (int max, string desc) GetPredInfo(string type, int val) {
            if(type.Contains("1bit")) return (1, val==0?"NT":"T");
            if(type.Contains("3bit")) return (7, val<4?"NT":"T");
            return (3, val<2?"NT":"T");
        }

        private static void HandleEdit(int idx, int dir, CliConfig c)
        {
            switch(idx) {
                case 1: c.Architecture = c.Architecture == "rv64" ? "rv32" : "rv64"; break;
                case 2: c.ExecutionMode = c.ExecutionMode == "pipeline" ? "simple" : "pipeline"; break;
                case 3: c.EnableTandemVerification = !c.EnableTandemVerification; break;
                case 5: c.PipelineWidth = Math.Clamp(c.PipelineWidth + dir, 1, 32); break;
                case 6: c.FetchBufferRatio = Math.Clamp(c.FetchBufferRatio + (dir * 0.5f), 1.0f, 8.0f); break; 
                case 7: c.EnableEarlyBranchResolution = !c.EnableEarlyBranchResolution; break; // Flush Penalty
                case 8: c.AllowDynamicBranchFetching = !c.AllowDynamicBranchFetching; break;
                case 9: c.AllowCascadedExecution = !c.AllowCascadedExecution; break;
                case 10: c.EnableMacroOpFusion = !c.EnableMacroOpFusion; break;
                case 11: c.EnableReturnAddressStack = !c.EnableReturnAddressStack; break;
                case 13: c.MaxCycles = Math.Max(1000, c.MaxCycles + (dir * 10000)); break;
                case 14: c.EnableProfiler = !c.EnableProfiler; break;
                case 15: c.SteppingMode = c.SteppingMode == "auto" ? "manual" : "auto"; break;
                case 16: 
                    var preds = new[] { "static", "bimodal-1bit", "bimodal-2bit", "bimodal-3bit", "gshare" };
                    int curP = Array.IndexOf(preds, c.BranchPredictor.ToLower());
                    if (curP == -1) curP = 0;
                    curP = (curP + dir + preds.Length) % preds.Length;
                    c.BranchPredictor = preds[curP]; c.PredictorInitValue = 0;
                    break;
                case 17: var (max,_) = GetPredInfo(c.BranchPredictor, c.PredictorInitValue); c.PredictorInitValue = Math.Clamp(c.PredictorInitValue + dir, 0, max); break;
                case 18: c.ShowRealTimeMetrics = !c.ShowRealTimeMetrics; break;
            }
        }
    }
}
