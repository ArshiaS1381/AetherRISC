using AetherRISC.Core.Abstractions.Interfaces;
using System;

namespace AetherRISC.Core.Architecture.Hardware.Pipeline.Predictors
{
    public class Bimodal3bPredictor : IBranchPredictor
    {
        public string Name => "Bimodal (3-Bit Saturating)";
        private const int TableSize = 4096;
        private const int Mask = TableSize - 1;
        
        private readonly byte[] _counters = new byte[TableSize];
        private readonly ulong[] _targetBuffer = new ulong[TableSize];
        private readonly ulong[] _tagBuffer = new ulong[TableSize];

        public Bimodal3bPredictor(int initialValue = 3)
        {
            byte safeValue = (byte)Math.Clamp(initialValue, 0, 7);
            Array.Fill(_counters, safeValue); 
        }

        public BranchPrediction Predict(ulong pc)
        {
            long index = (long)((pc >> 1) & Mask);
            if (_tagBuffer[index] != pc) return new BranchPrediction { PredictedTaken = false, TargetAddress = 0 };

            byte state = _counters[index];
            bool take = state >= 4; // Threshold for 3-bit

            return new BranchPrediction { PredictedTaken = take, TargetAddress = take ? _targetBuffer[index] : 0 };
        }

        public void Update(ulong pc, bool taken, ulong target)
        {
            long index = (long)((pc >> 1) & Mask);
            _tagBuffer[index] = pc;
            if (taken) _targetBuffer[index] = target;

            byte state = _counters[index];
            if (taken) { if (state < 7) state++; }
            else       { if (state > 0) state--; }
            _counters[index] = state;
        }
    }
}
