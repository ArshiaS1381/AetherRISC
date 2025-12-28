using System;

namespace AetherRISC.Core.Assembler.Directives
{
    public class TextDirective : IAssemblerDirective
    {
        public string Name => ".text";
        public string Description => "Switches to the Text section. This is where executable code lives. Instructions placed here are read-only and executable.";
        public string Usage => ".text";

        public bool Match(string token) => token.Equals(Name, StringComparison.OrdinalIgnoreCase);

        public void Execute(SourceAssembler asm, string[] args)
        {
            // Switch pointer context to Text
            asm.SwitchSection(SectionType.Text);
            
            // Instructions require 4-byte alignment
            asm.Align(2); 
        }
    }
}
