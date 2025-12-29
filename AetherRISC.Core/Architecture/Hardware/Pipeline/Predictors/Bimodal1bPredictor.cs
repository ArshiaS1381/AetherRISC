using AetherRISC.Core.Abstractions.Interfaces;
using System;

namespace AetherRISC.Core.Architecture.Hardware.Pipeline.Predictors
{
    /// <summary>
    /// A simple Last-Outcome predictor.
    /// Memory: 1 bit per entry.
    /// Logic: If last was taken, predict taken. If last was not taken, predict not taken.
    /// </summary>
    public class Bimodal1bPredictor : IBranchPredictor
    {
        public string Name => "Bimodal (1-Bit / Last Outcome)";

        private const int TableSize = 4096;
        private const int Mask = TableSize - 1;
        
        // 0 = Not Taken, 1 = Taken
        private readonly byte[] _counters = new byte[TableSize];
        
        // BTB for Target Resolution
        private readonly ulong[] _targetBuffer = new ulong[TableSize];
        private readonly ulong[] _tagBuffer = new ulong[TableSize];

        // UPDATED: Constructor now accepts initial value
        public Bimodal1bPredictor(int initialValue = 0)
        {
            // Clamp to valid 1-bit range (0-1)
            byte safeValue = (byte)Math.Clamp(initialValue, 0, 1);
            Array.Fill(_counters, safeValue);
        }

        public BranchPrediction Predict(ulong pc)
        {
            long index = (long)((pc >> 1) & Mask);

            if (_tagBuffer[index] != pc)
            {
                return new BranchPrediction { PredictedTaken = false, TargetAddress = 0 };
            }

            bool take = _counters[index] == 1;

            return new BranchPrediction 
            { 
                PredictedTaken = take, 
                TargetAddress = take ? _targetBuffer[index] : 0 
            };
        }

        public void Update(ulong pc, bool taken, ulong target)
        {
            long index = (long)((pc >> 1) & Mask);

            _tagBuffer[index] = pc;
            if (taken) _targetBuffer[index] = target;

            _counters[index] = taken ? (byte)1 : (byte)0;
        }
    }
}
