using System;

namespace AetherRISC.Core.Assembler.Directives
{
    public class GlobalDirective : IAssemblerDirective
    {
        public string Name => ".globl";
        public string Description => "Declares a symbol as global, making it visible to the linker. In this simulator, it marks symbols as exportable.";
        public string Usage => ".globl <symbol_name>";

        public bool Match(string token) => token.Equals(Name, StringComparison.OrdinalIgnoreCase) || token.Equals(".global", StringComparison.OrdinalIgnoreCase);

        public void Execute(SourceAssembler asm, string[] args)
        {
            // Currently treated as a no-op in single-file assembly, 
            // but reserved for future linker support.
        }
    }
}
