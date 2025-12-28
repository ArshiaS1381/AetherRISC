using System;

namespace AetherRISC.Core.Assembler.Directives
{
    public class DataDirective : IAssemblerDirective
    {
        public string Name => ".data";
        public string Description => "Switches to the Data section. This is for Read/Write variables. Use this for global variables, arrays, or mutable strings.";
        public string Usage => ".data";

        public bool Match(string token) => token.Equals(Name, StringComparison.OrdinalIgnoreCase);

        public void Execute(SourceAssembler asm, string[] args)
        {
            asm.SwitchSection(SectionType.Data);
        }
    }
}
