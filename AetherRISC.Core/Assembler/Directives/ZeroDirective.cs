using System;

namespace AetherRISC.Core.Assembler.Directives
{
    public class ZeroDirective : IAssemblerDirective
    {
        public string Name => ".zero";
        public string Description => "Alias for .space. Allocates a specific number of zeroed bytes.";
        public string Usage => ".zero <bytes>";

        public bool Match(string token) => token.Equals(Name, StringComparison.OrdinalIgnoreCase);

        public void Execute(SourceAssembler asm, string[] args)
        {
            if (args.Length > 0 && asm.TryParseNumber(args[0], out long bytes))
            {
                for (int i = 0; i < bytes; i++) asm.WriteByte(0);
            }
        }
    }
}
