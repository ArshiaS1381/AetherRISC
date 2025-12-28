using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AetherRISC.Core.Abstractions.Interfaces;

namespace AetherRISC.Core.Architecture.Hardware.ISA.Pseudo;

public static class PseudoExpander
{
    private static readonly Dictionary<string, IPseudoInstruction> _pseudos = new(StringComparer.OrdinalIgnoreCase);

    static PseudoExpander()
    {
        // Scan ALL loaded assemblies to find pseudo implementations.
        // This fixes issues where Core is loaded in a Test context.
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        
        foreach (var asm in assemblies)
        {
            try 
            {
                var types = asm.GetTypes()
                    .Where(t => typeof(IPseudoInstruction).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

                foreach (var t in types)
                {
                    if (Activator.CreateInstance(t) is IPseudoInstruction inst)
                    {
                        _pseudos[inst.Mnemonic] = inst;
                    }
                }
            }
            catch { /* Ignore assemblies that fail to load types */ }
        }
    }

    public static IEnumerable<IInstruction> Expand(
        string mnemonic,
        int rd,
        int rs1,
        int rs2,
        long imm
    )
    {
        if (_pseudos.TryGetValue(mnemonic, out var pseudo))
        {
            return pseudo.Expand(rd, rs1, rs2, imm);
        }
        return Enumerable.Empty<IInstruction>();
    }
}
