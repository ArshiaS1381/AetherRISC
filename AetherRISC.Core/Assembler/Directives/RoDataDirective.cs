using System;

namespace AetherRISC.Core.Assembler.Directives
{
    public class RoDataDirective : IAssemblerDirective
    {
        public string Name => ".rodata";
        public string Description => "Switches to the Read-Only Data section. Use this for constants and immutable strings. Writes to this memory at runtime will trigger a fault.";
        public string Usage => ".rodata";

        public bool Match(string token) => token.Equals(Name, StringComparison.OrdinalIgnoreCase);

        public void Execute(SourceAssembler asm, string[] args)
        {
            // In this sim, we treat it like Data but logically track it as RoData
            asm.SwitchSection(SectionType.RoData);
        }
    }
}
