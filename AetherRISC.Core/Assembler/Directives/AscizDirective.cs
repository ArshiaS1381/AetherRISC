using System;
using System.Linq;

namespace AetherRISC.Core.Assembler.Directives
{
    /// <summary>
    /// Directive to store null-terminated strings.
    /// </summary>
    public class AscizDirective : IAssemblerDirective
    {
        public string Name => ".asciz";
        public string Description => "Stores a null-terminated C-style string in memory. Useful for strings passed to syscalls like PrintString. Supports escape sequences like \\n (newline) and \\t (tab).";
        public string Usage => ".asciz \"string\"";

        public bool Match(string token) => 
            token.Equals(Name, StringComparison.OrdinalIgnoreCase) || 
            token.Equals(".string", StringComparison.OrdinalIgnoreCase);

        public void Execute(SourceAssembler asm, string[] args)
        {
            if (args.Length == 0) return;
            
            // Rejoin arguments to handle strings containing spaces
            string raw = string.Join(" ", args);
            
            // FIX: Correctly trim both double and single quotes
            string content = raw.Trim('\"', '\'');
            
            // Process standard escape sequences
            content = content
                .Replace("\\n", "\n")
                .Replace("\\r", "\r")
                .Replace("\\t", "\t")
                .Replace("\\0", "\0")
                .Replace("\\\"", "\"");

            // Write each character as a byte
            foreach (char c in content)
            {
                asm.WriteByte((byte)c);
            }
            
            // Append the mandatory Null Terminator
            asm.WriteByte(0); 
        }
    }
}
