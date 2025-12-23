using System;
using System.Collections.Generic;
using AetherRISC.Core.Abstractions.Interfaces;

namespace AetherRISC.Core.Helpers
{
    public class TestAssembler
    {
        private readonly List<(string? label, Func<int, IInstruction> factory)> _lines = new();
        private readonly Dictionary<string, int> _labels = new();

        public void Add(Func<int, IInstruction> factory, string? label = null)
        {
            if (!string.IsNullOrEmpty(label)) 
            {
                _labels[label] = _lines.Count * 4;
            }
            _lines.Add((label, factory));
        }

        public List<IInstruction> Assemble()
        {
            var result = new List<IInstruction>();
            for (int i = 0; i < _lines.Count; i++) 
            {
                // Pass current PC (i * 4) to the factory for PC-relative calculations
                result.Add(_lines[i].factory(i * 4));
            }
            return result;
        }

        // Calculates offset from currentPc to target label
        public int To(string label, int currentPc) 
        {
            return _labels.ContainsKey(label) ? _labels[label] - currentPc : 0;
        }
    }
}
