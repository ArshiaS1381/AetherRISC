using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;

namespace AetherRISC.CLI
{
    class Program
    {
        static void Main(string[] args)
        {
            Setup();
            var loader = new ProgramLoader("config.json");

            while (true)
            {
                var c = loader.Config;
                // Colors
                string C_Green = "\u001b[32m"; string C_Red = "\u001b[31m";
                string C_Cyan = "\u001b[36m"; string C_Yel = "\u001b[33m";
                string C_Mag = "\u001b[35m"; string C_Gray = "\u001b[90m";
                string C_Reset = "\u001b[0m";

                string prof = c.EnableProfiler ? $"{C_Green}ON " : $"{C_Red}OFF";
                string w = c.ExecutionMode == "pipeline" ? $"{C_Cyan}{c.PipelineWidth}x" : $"{C_Gray}1x ";
                string arch = $"{C_Yel}{c.Architecture.ToUpper()}";
                string mode = $"{C_Mag}{c.ExecutionMode.ToUpper()}";

                string info = $" {arch} {C_Gray}|{C_Reset} {mode} {C_Gray}|{C_Reset} {w} {C_Gray}|{C_Reset} Prof:{prof}{C_Reset}";
                
                int sel = ShowMenu("AetherRISC v5.0", info, new[] { "Run Program", "Settings", "Quit" });

                if (sel == 0) ShowFiles(loader);
                else if (sel == 1) ConfigurationMenu.Show(loader);
                else if (sel == 2) break;
                
                Console.Clear();
            }
        }

        static void ShowFiles(ProgramLoader loader) {
            var files = loader.GetAvailablePrograms();
            var names = files.Select(Path.GetFileName).Concat(new[] { "Back" }).ToArray();
            int sel = ShowMenu("Select Program", "", names!);
            if (sel != -1 && sel != names.Length - 1) SimulationRunner.Run(loader, files[sel]);
        }

        static int ShowMenu(string t, string s, string[] o) {
            int sel = 0;
            // Colors
            string C_Frame = "\u001b[38;5;239m"; string C_Title = "\u001b[38;5;39m";
            string C_White = "\u001b[97m"; string C_Reset = "\u001b[0m";
            string C_SelBg = "\u001b[48;5;237m"; string C_SelFg = "\u001b[38;5;255m";
            string C_Shadow = "\u001b[48;5;232m ";

            while(true) {
                Console.SetCursorPosition(0,0);
                
                int w = 60;
                int left = Math.Max(0, (Console.WindowWidth - w) / 2);
                string padL = new string(' ', left);
                string padShadow = new string(' ', left + 2);

                StringBuilder sb = new StringBuilder();
                sb.Append("\n\n");
                
                // Top Border
                string cleanTitle = Regex.Replace(t, "\u001b\\[[0-9;]*m", "");
                sb.Append($"{padL}{C_Frame}┌─ {C_Title}{t} {C_Frame}" + new string('─', Math.Max(0, w - cleanTitle.Length - 5)) + "┐\n");
                
                // Info Section
                if(!string.IsNullOrEmpty(s)) {
                   int visLen = Regex.Replace(s, "\u001b\\[[0-9;]*m", "").Length;
                   int pad = Math.Max(0, w - visLen - 3);
                   sb.Append($"{padL}{C_Frame}│ {s}" + new string(' ', pad) + $"{C_Frame}│{C_Shadow}{C_Reset}\n");
                   sb.Append($"{padL}{C_Frame}├" + new string('─', w - 2) + $"┤{C_Shadow}{C_Reset}\n");
                } else {
                   sb.Append($"{padL}{C_Frame}├" + new string('─', w - 2) + $"┤{C_Shadow}{C_Reset}\n");
                }

                // Options with Strict Padding
                for(int i=0; i<o.Length; i++) {
                    sb.Append($"{padL}{C_Frame}│ ");
                    string label = o[i];
                    int cleanLen = label.Length; // filenames usually don't have ansi
                    
                    if (i==sel) {
                        int pad = Math.Max(0, w - 7 - cleanLen);
                        sb.Append($"{C_SelBg}{C_SelFg} > {label}{new string(' ', pad)} {C_Reset}");
                    } else {
                        int pad = Math.Max(0, w - 7 - cleanLen);
                        sb.Append($"   {label}{new string(' ', pad)} ");
                    }
                    
                    sb.Append($"{C_Frame}│{C_Shadow}{C_Reset}\n");
                }
                
                // Bottom
                sb.Append($"{padL}{C_Frame}└" + new string('─', w - 2) + $"┘{C_Shadow}{C_Reset}\n");
                sb.Append($"{padShadow}{new string(' ', w)}{C_Reset}\n\n");
                
                for(int i=0; i<5; i++) sb.Append(new string(' ', Console.WindowWidth) + "\n");

                Console.SetCursorPosition(0,0);
                Console.Write(sb.ToString());

                var k = Console.ReadKey(true).Key;
                if (k == ConsoleKey.UpArrow) { sel--; if (sel < 0) sel = o.Length - 1; }
                else if (k == ConsoleKey.DownArrow) { sel++; if (sel >= o.Length) sel = 0; }
                else if (k == ConsoleKey.Enter) return sel;
                else if (k == ConsoleKey.Escape) return -1;
            }
        }

        static void Setup() {
            Console.OutputEncoding = Encoding.UTF8;
            Console.CursorVisible = false;
            Console.Clear();
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                try { Console.WindowWidth = Math.Max(Console.WindowWidth, 120); Console.WindowHeight = Math.Max(Console.WindowHeight, 50); } catch {}
            }
        }
    }
}
