using AetherRISC.Core.Abstractions.Interfaces;
using System;

namespace AetherRISC.Core.Architecture.Hardware.Pipeline.Predictors
{
    public class BimodalPredictor : IBranchPredictor
    {
        public string Name => "Bimodal (2-Bit Saturating)";

        private const int TableSize = 4096;
        private const int Mask = TableSize - 1;
        
        // 0=StrongNT, 1=WeakNT, 2=WeakT, 3=StrongT
        private readonly byte[] _counters = new byte[TableSize];
        
        private readonly ulong[] _targetBuffer = new ulong[TableSize];
        private readonly ulong[] _tagBuffer = new ulong[TableSize];

        public BimodalPredictor()
        {
            Array.Fill(_counters, (byte)1); // Init Weakly Not Taken
        }

        public BranchPrediction Predict(ulong pc)
        {
            // FIX: Use >> 1 instead of >> 2. RISC-V supports 2-byte compressed instructions.
            // >> 2 causes aliasing between PC and PC+2.
            long index = (long)((pc >> 1) & Mask);

            if (_tagBuffer[index] != pc)
            {
                return new BranchPrediction { PredictedTaken = false, TargetAddress = 0 };
            }

            byte state = _counters[index];
            bool take = state >= 2;

            return new BranchPrediction 
            { 
                PredictedTaken = take, 
                TargetAddress = take ? _targetBuffer[index] : 0 
            };
        }

        public void Update(ulong pc, bool taken, ulong target)
        {
            long index = (long)((pc >> 1) & Mask);

            // Update BTB
            _tagBuffer[index] = pc;
            if (taken) _targetBuffer[index] = target;

            // Update Saturating Counter
            byte state = _counters[index];
            if (taken)
            {
                if (state < 3) state++;
            }
            else
            {
                if (state > 0) state--;
            }
            _counters[index] = state;
        }
    }
}
