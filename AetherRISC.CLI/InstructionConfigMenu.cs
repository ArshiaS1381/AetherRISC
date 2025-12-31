using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AetherRISC.CLI
{
    public static class InstructionConfigMenu
    {
        public static void Show(CliConfig config)
        {
            var allInsts = InstructionRepository.GetAll();
            int sel = 0;
            int scroll = 0;
            const int W = 74; 
            
            string C_Frame = "\u001b[38;5;239m"; string C_Title = "\u001b[38;5;39m";
            string C_White = "\u001b[38;5;255m"; string C_Dim = "\u001b[38;5;240m";
            string C_SelBg = "\u001b[48;5;237m"; string C_SelFg = "\u001b[38;5;255m";
            string C_Green = "\u001b[38;5;46m";  string C_Red = "\u001b[38;5;196m";
            string C_Reset = "\u001b[0m"; string C_Shadow = "\u001b[48;5;232m ";
            string C_Cat = "\u001b[38;5;214m"; 

            Console.Clear();

            while (true)
            {
                // Full Screen Buffer Construction
                int screenH = Console.WindowHeight;
                int screenW = Console.WindowWidth;
                int pageHeight = Math.Max(5, screenH - 10);
                int left = Math.Max(0, (screenW - W) / 2);
                string padL = new string(' ', left);
                
                StringBuilder sb = new StringBuilder();
                
                // Add empty header lines to vertically center if needed
                sb.Append("\n\n");
                
                sb.Append($"{padL}{C_Frame}┌─ {C_Title}INSTRUCTION SET CONFIG {C_Frame}" + new string('─', W - 27) + "┐\n");
                string help = "Space: Toggle | Enter: Return";
                sb.Append($"{padL}{C_Frame}│ {C_Dim}{help}" + new string(' ', W - 4 - help.Length) + $"{C_Frame}│{C_Shadow}\n");
                sb.Append($"{padL}{C_Frame}├" + new string('─', W - 2) + $"┤{C_Shadow}\n");

                if (sel < scroll) scroll = sel;
                if (sel >= scroll + pageHeight) scroll = sel - pageHeight + 1;
                scroll = Math.Clamp(scroll, 0, Math.Max(0, allInsts.Count - pageHeight));

                for (int i = 0; i < pageHeight; i++)
                {
                    int idx = scroll + i;
                    sb.Append($"{padL}{C_Frame}│ "); 

                    if (idx < allInsts.Count)
                    {
                        var inst = allInsts[idx];
                        bool isDisabled = config.DisabledInstructions.Contains(inst.Mnemonic);
                        string status = isDisabled ? $"{C_Red}[DISABLED]" : $"{C_Green}[ ENABLED]";
                        
                        string name = inst.Mnemonic.Length > 9 ? inst.Mnemonic.Substring(0,9) : inst.Mnemonic;
                        string cat = inst.Family.Length > 15 ? inst.Family.Substring(0,15) : inst.Family;

                        string ptr = (idx == sel) ? ">" : " ";
                        string line = $"{ptr} {name,-10} {C_Dim}| {C_Cat}{cat,-17} {C_Dim}| {status}";
                        int visLen = 45; 
                        int padding = (W - 4) - visLen;

                        if (idx == sel)
                            sb.Append($"{C_SelBg}{C_SelFg}{line}{new string(' ', Math.Max(0, padding))}{C_Reset}");
                        else
                            sb.Append($" {line}{new string(' ', Math.Max(0, padding))} ");
                    }
                    else
                    {
                        sb.Append(new string(' ', W - 4));
                    }
                    sb.Append($"{C_Frame}│{C_Shadow}{C_Reset}\n"); 
                }

                sb.Append($"{padL}{C_Frame}└" + new string('─', W - 2) + $"┘{C_Shadow}{C_Reset}\n");
                
                if (allInsts.Count > 0) {
                    double pct = (double)sel / (allInsts.Count - 1);
                    int barW = W - 4;
                    int pos = (int)(pct * barW);
                    sb.Append($"{padL} {C_Frame}[{C_Green}{new string('=', pos)}{C_Frame}{new string('-', Math.Max(0, barW - pos))}{C_Frame}]{C_Reset}\n");
                }

                // Explicitly wipe the rest of the screen to avoid artifacts
                string clearLine = new string(' ', screenW);
                int linesUsed = 2 + 1 + 1 + 1 + pageHeight + 1 + 1; // Approx
                for(int k=0; k < (screenH - linesUsed); k++) sb.Append(clearLine + "\n");
                
                Console.SetCursorPosition(0,0);
                Console.Write(sb.ToString());

                var key = Console.ReadKey(true).Key;

                if (key == ConsoleKey.UpArrow) sel = Math.Max(0, sel - 1);
                else if (key == ConsoleKey.DownArrow) sel = Math.Min(allInsts.Count - 1, sel + 1);
                else if (key == ConsoleKey.PageUp) sel = Math.Max(0, sel - pageHeight);
                else if (key == ConsoleKey.PageDown) sel = Math.Min(allInsts.Count - 1, sel + pageHeight);
                else if (key == ConsoleKey.Home) sel = 0;
                else if (key == ConsoleKey.End) sel = allInsts.Count - 1;
                else if (key == ConsoleKey.Spacebar || key == ConsoleKey.RightArrow || key == ConsoleKey.LeftArrow)
                {
                    if (allInsts.Count > 0) {
                        var m = allInsts[sel].Mnemonic;
                        if (config.DisabledInstructions.Contains(m)) config.DisabledInstructions.Remove(m);
                        else config.DisabledInstructions.Add(m);
                    }
                }
                else if (key == ConsoleKey.Enter || key == ConsoleKey.Escape) {
                    Console.Clear(); 
                    break;
                }
            }
        }
    }
}
