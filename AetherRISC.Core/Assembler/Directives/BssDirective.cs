using System;

namespace AetherRISC.Core.Assembler.Directives
{
    public class BssDirective : IAssemblerDirective
    {
        public string Name => ".bss";
        public string Description => "Switches to the BSS section (Block Started by Symbol). This area is reserved for uninitialized variables and is automatically zeroed out at program start.";
        public string Usage => ".bss";

        public bool Match(string token) => token.Equals(Name, StringComparison.OrdinalIgnoreCase);

        public void Execute(SourceAssembler asm, string[] args)
        {
            // Map BSS to Data section, usually aligned to 8 bytes
            asm.SwitchSection(SectionType.Data);
            asm.Align(3); 
        }
    }
}
