using System;

namespace AetherRISC.Core.Assembler.Directives
{
    public class SpaceDirective : IAssemblerDirective
    {
        public string Name => ".space";
        public string Description => "Reserves a block of memory with a specified size in bytes. The reserved memory is initialized to zero.";
        public string Usage => ".space <bytes>";

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
