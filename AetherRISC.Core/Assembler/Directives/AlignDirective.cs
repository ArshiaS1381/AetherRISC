using System;

namespace AetherRISC.Core.Assembler.Directives
{
    public class AlignDirective : IAssemblerDirective
    {
        public string Name => ".align";
        public string Description => "Aligns the memory pointer to the next boundary of 2^N bytes. For example, .align 2 aligns to a 4-byte boundary.";
        public string Usage => ".align <power>";

        public bool Match(string token) => token.Equals(Name, StringComparison.OrdinalIgnoreCase);

        public void Execute(SourceAssembler asm, string[] args)
        {
            // .align N means align to 2^N bytes
            if (args.Length > 0 && asm.TryParseNumber(args[0], out long power))
            {
                asm.Align((int)power);
            }
        }
    }
}
