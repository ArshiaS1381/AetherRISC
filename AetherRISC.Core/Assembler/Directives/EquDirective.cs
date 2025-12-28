using System;

namespace AetherRISC.Core.Assembler.Directives
{
    public class EquDirective : IAssemblerDirective
    {
        public string Name => ".equ";
        public string Description => "Defines a constant value for a symbol. This value is substituted wherever the symbol is used.";
        public string Usage => ".equ <name>, <value>";

        public bool Match(string token) => token.Equals(Name, StringComparison.OrdinalIgnoreCase) || token.Equals(".eqv", StringComparison.OrdinalIgnoreCase);

        public void Execute(SourceAssembler asm, string[] args)
        {
            if (args.Length >= 2 && asm.TryParseNumber(args[1], out long val))
            {
                // Remove trailing commas if present in the syntax (e.g. .equ NAME, VAL)
                string label = args[0].TrimEnd(',');
                asm.DefineConstant(label, (uint)val);
            }
        }
    }
}
