using System;

namespace AetherRISC.Core.Assembler.Directives
{
    public class DwordDirective : IAssemblerDirective
    {
        public string Name => ".dword";
        public string Description => "Stores one or more 64-bit values (double words) in memory. Automatically aligns to an 8-byte boundary.";
        public string Usage => ".dword value1, value2, ...";

        public bool Match(string token) => token.Equals(Name, StringComparison.OrdinalIgnoreCase) || token.Equals(".quad", StringComparison.OrdinalIgnoreCase);

        public void Execute(SourceAssembler asm, string[] args)
        {
            // Ensure 8-byte alignment
            asm.Align(3); 
            
            foreach (var arg in args)
            {
                if (asm.TryParseNumber(arg, out long val))
                {
                    // Write 64-bit value as two 32-bit words (Little Endian)
                    asm.WriteWord((uint)(val & 0xFFFFFFFF));
                    asm.WriteWord((uint)((val >> 32) & 0xFFFFFFFF));
                }
                else
                {
                    // Handle Label References (pointer)
                    uint addr = asm.ResolveSymbol(arg);
                    asm.WriteWord(addr);
                    asm.WriteWord(0); // Upper 32-bits are 0 in this 32-bit address space
                }
            }
        }
    }
}
