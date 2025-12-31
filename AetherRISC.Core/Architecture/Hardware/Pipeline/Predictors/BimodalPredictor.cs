using AetherRISC.Core.Abstractions.Interfaces;
using System;

namespace AetherRISC.Core.Architecture.Hardware.Pipeline.Predictors
{
    public class TunableBimodalPredictor : IBranchPredictor
    {
        // FIXED: Added fields for property usage
        private readonly int _sizeBits;
        private readonly int _counterBits;
        
        public string Name => $"Bimodal ({_counterBits}-bit, 2^{_sizeBits} entries)";

        private readonly byte[] _counters;
        private readonly ulong[] _targetBuffer;
        private readonly ulong[] _tagBuffer;
        private readonly int _mask;
        private readonly int _maxVal;
        private readonly int _threshold;

        public TunableBimodalPredictor(int sizeBits, int counterBits, int initialVal)
        {
            _sizeBits = sizeBits;
            _counterBits = counterBits;

            int size = 1 << sizeBits;
            _mask = size - 1;
            _maxVal = (1 << counterBits) - 1;
            _threshold = 1 << (counterBits - 1);

            _counters = new byte[size];
            _targetBuffer = new ulong[size];
            _tagBuffer = new ulong[size];

            byte safeInit = (byte)Math.Clamp(initialVal, 0, _maxVal);
            Array.Fill(_counters, safeInit);
        }

        public BranchPrediction Predict(ulong pc)
        {
            // FIXED: Explicit cast to ulong to match PC type
            long index = (long)((pc >> 1) & (ulong)_mask);
            
            if (_tagBuffer[index] != pc) 
                return new BranchPrediction { PredictedTaken = false, TargetAddress = 0 };

            bool take = _counters[index] >= _threshold;
            return new BranchPrediction { PredictedTaken = take, TargetAddress = take ? _targetBuffer[index] : 0 };
        }

        public void Update(ulong pc, bool taken, ulong target)
        {
            // FIXED: Explicit cast
            long index = (long)((pc >> 1) & (ulong)_mask);
            _tagBuffer[index] = pc;
            if (taken) _targetBuffer[index] = target;

            byte val = _counters[index];
            if (taken) {
                if (val < _maxVal) val++;
            } else {
                if (val > 0) val--;
            }
            _counters[index] = val;
        }
    }
}
