using System;
using System.Collections.Generic;
using AetherRISC.Core.Abstractions.Interfaces;

namespace AetherRISC.CLI;

public class LabelAssembler
{
    private readonly List<(string? label, Func<int, IInstruction> factory)> _lines = new();
    private readonly Dictionary<string, int> _labels = new();

    public void Add(Func<int, IInstruction> factory, string? label = null)
    {
        if (!string.IsNullOrEmpty(label)) 
        {
            if (_labels.ContainsKey(label)) 
                throw new InvalidOperationException($"Duplicate label definition: {label}");
            
            _labels[label] = _lines.Count * 4;
        }
        _lines.Add((label, factory));
    }

    public List<IInstruction> Assemble()
    {
        var result = new List<IInstruction>();
        for (int i = 0; i < _lines.Count; i++)
        {
            result.Add(_lines[i].factory(i * 4));
        }
        return result;
    }

    public int To(string label, int currentPc) 
    {
        if (!_labels.ContainsKey(label)) 
            throw new InvalidOperationException($"Undefined label reference: {label} at PC {currentPc}");
        
        return _labels[label] - currentPc;
    }
}
