using System;

namespace AetherRISC.Core.Assembler.Directives
{
    public class ByteDirective : IAssemblerDirective
    {
        public string Name => ".byte";
        public string Description => "Stores one or more 8-bit values in memory. Values exceeding 255 are truncated.";
        public string Usage => ".byte value1, value2, ...";

        public bool Match(string token) => token.Equals(Name, StringComparison.OrdinalIgnoreCase);

        public void Execute(SourceAssembler asm, string[] args)
        {
            foreach (var arg in args)
            {
                if (asm.TryParseNumber(arg, out long val))
                {
                    asm.WriteByte((byte)val);
                }
            }
        }
    }
}
