using AetherRISC.Core.Abstractions.Interfaces;
using System;

namespace AetherRISC.Core.Architecture.Hardware.Pipeline.Predictors
{
    public class TunableGSharePredictor : IBranchPredictor
    {
        // FIXED: Added missing fields
        private readonly int _historyBits;
        private readonly int _tableBits;
        
        public string Name => $"GShare (H:{_historyBits}, T:{_tableBits})";

        private readonly byte[] _pht; 
        private readonly ulong[] _btb; 
        private readonly ulong[] _tag; 
        
        private readonly int _mask;
        private uint _globalHistory;

        public TunableGSharePredictor(int historyBits, int tableBits)
        {
            _historyBits = historyBits;
            _tableBits = tableBits;
            
            int size = 1 << tableBits;
            _mask = size - 1;

            _pht = new byte[size];
            _btb = new ulong[size];
            _tag = new ulong[size];

            Array.Fill(_pht, (byte)1); 
        }

        public BranchPrediction Predict(ulong pc)
        {
            // FIXED: Explicit casts for ulong & int mixing
            uint pcPart = (uint)((pc >> 1) & (ulong)_mask);
            uint index = (pcPart ^ _globalHistory) & (uint)_mask;

            if (_tag[pcPart] != pc) 
                return new BranchPrediction { PredictedTaken = false, TargetAddress = 0 };

            bool take = _pht[index] >= 2;
            return new BranchPrediction { PredictedTaken = take, TargetAddress = take ? _btb[pcPart] : 0 };
        }

        public void Update(ulong pc, bool taken, ulong target)
        {
            // FIXED: Explicit casts
            uint pcPart = (uint)((pc >> 1) & (ulong)_mask);
            uint index = (pcPart ^ _globalHistory) & (uint)_mask;

            byte state = _pht[index];
            if (taken) { if (state < 3) state++; }
            else       { if (state > 0) state--; }
            _pht[index] = state;

            _tag[pcPart] = pc;
            if (taken) _btb[pcPart] = target;

            _globalHistory = (_globalHistory << 1) | (taken ? 1u : 0u);
            _globalHistory &= (uint)((1 << _historyBits) - 1);
        }
    }
}
