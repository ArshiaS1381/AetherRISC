using System;

namespace AetherRISC.Core.Assembler.Directives
{
    public class WordDirective : IAssemblerDirective
    {
        public string Name => ".word";
        public string Description => "Stores one or more 32-bit values in memory. Automatically aligns to a 4-byte boundary. Can also store the address of a label.";
        public string Usage => ".word value_or_label, ...";

        public bool Match(string token) => token.Equals(Name, StringComparison.OrdinalIgnoreCase);

        public void Execute(SourceAssembler asm, string[] args)
        {
            // Ensure 4-byte alignment
            asm.Align(2);

            foreach (var arg in args)
            {
                // Try to parse as a number literal first
                if (asm.TryParseNumber(arg, out long val))
                {
                    asm.WriteWord((uint)val);
                }
                else
                {
                    // Assume it is a label reference; resolve its address
                    uint addr = asm.ResolveSymbol(arg); 
                    asm.WriteWord(addr);
                }
            }
        }
    }
}
