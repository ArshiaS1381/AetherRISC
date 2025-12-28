using System;

namespace AetherRISC.Core.Assembler.Directives
{
    public class HalfDirective : IAssemblerDirective
    {
        public string Name => ".half";
        public string Description => "Stores one or more 16-bit (half-word) values in memory. Automatically aligns to a 2-byte boundary.";
        public string Usage => ".half value1, value2, ...";

        public bool Match(string token) => token.Equals(Name, StringComparison.OrdinalIgnoreCase);

        public void Execute(SourceAssembler asm, string[] args)
        {
            // Ensure 2-byte alignment
            asm.Align(1);

            foreach (var arg in args)
            {
                if (asm.TryParseNumber(arg, out long val))
                {
                    asm.WriteHalf((ushort)val);
                }
            }
        }
    }
}
