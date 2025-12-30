using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AetherRISC.Core.Abstractions.Interfaces; // Added namespace
using AetherRISC.Core.Architecture.Hardware.ISA;

namespace AetherRISC.CLI
{
    public class InstructionMetadata
    {
        public required string Mnemonic { get; set; }
        public required string Family { get; set; }
        public required string Description { get; set; }
        public InstructionSet Set { get; set; }
    }

    public static class InstructionRepository
    {
        // Fix: Make nullable or use ! since we init in method
        private static List<InstructionMetadata>? _cache;

        public static List<InstructionMetadata> GetAll()
        {
            if (_cache != null) return _cache;

            _cache = new List<InstructionMetadata>();
            var asm = typeof(IInstruction).Assembly;
            
            var types = asm.GetTypes()
                           .Where(t => t.GetCustomAttributes<RiscvInstructionAttribute>().Any());

            foreach (var t in types)
            {
                var attrs = t.GetCustomAttributes<RiscvInstructionAttribute>();
                foreach(var attr in attrs)
                {
                    _cache.Add(new InstructionMetadata
                    {
                        Mnemonic = attr.Mnemonic,
                        Family = FormatSet(attr.Architecture),
                        Set = attr.Architecture,
                        Description = attr.Description ?? ""
                    });
                }
            }

            return _cache.OrderBy(x => x.Family).ThenBy(x => x.Mnemonic).ToList();
        }

        private static string FormatSet(InstructionSet s)
        {
            if (s.HasFlag(InstructionSet.RV64I) || s.HasFlag(InstructionSet.RV32I)) return "Base Integer";
            if (s.HasFlag(InstructionSet.RV64M)) return "M (Mul/Div)";
            if (s.HasFlag(InstructionSet.RV64A)) return "A (Atomic)";
            if (s.HasFlag(InstructionSet.RV64F) || s.HasFlag(InstructionSet.RV64D)) return "F/D (Float)";
            if (s.HasFlag(InstructionSet.Zicsr)) return "System (CSR)";
            return s.ToString();
        }
    }
}
