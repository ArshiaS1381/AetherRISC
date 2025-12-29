using AetherRISC.Core.Abstractions.Interfaces;
using System;

namespace AetherRISC.Core.Architecture.Hardware.Pipeline.Predictors
{
    public class GsharePredictor : IBranchPredictor
    {
        public string Name => "Gshare (Global History)";

        private const int HistoryBits = 12; 
        private const int TableSize = 1 << HistoryBits;
        private const int Mask = TableSize - 1;

        private readonly byte[] _pht = new byte[TableSize];
        private uint _globalHistory = 0;

        // FIX: Added BTB. Gshare predicts direction, but needs a BTB for the Target.
        private readonly ulong[] _targetBuffer = new ulong[TableSize];
        private readonly ulong[] _tagBuffer = new ulong[TableSize];

        public GsharePredictor()
        {
            Array.Fill(_pht, (byte)1); 
        }

        public BranchPrediction Predict(ulong currentPC)
        {
            // 1. Prediction (Direction)
            // Use >> 1 for Compressed support
            uint pcPart = (uint)((currentPC >> 1) & Mask);
            uint index = pcPart ^ _globalHistory;
            
            byte state = _pht[index];
            bool predictTaken = state >= 2;

            // 2. Target Resolution (BTB)
            // We use standard PC indexing for BTB (uncorrelated), 
            // though some impls use Gshare index for BTB too. Simple is better here.
            long btbIndex = (long)((currentPC >> 1) & Mask);
            
            if (_tagBuffer[btbIndex] != currentPC)
            {
                // Unknown branch? Predict NT safely.
                return new BranchPrediction { PredictedTaken = false, TargetAddress = 0 };
            }

            return new BranchPrediction 
            { 
                PredictedTaken = predictTaken, 
                TargetAddress = predictTaken ? _targetBuffer[btbIndex] : 0 
            };
        }

        public void Update(ulong branchPC, bool actuallyTaken, ulong actualTarget)
        {
            uint pcPart = (uint)((branchPC >> 1) & Mask);
            uint index = pcPart ^ _globalHistory;

            // Update PHT
            byte state = _pht[index];
            if (actuallyTaken) { if (state < 3) state++; }
            else { if (state > 0) state--; }
            _pht[index] = state;

            // Update Global History
            _globalHistory = (_globalHistory << 1) | (actuallyTaken ? 1u : 0u);
            _globalHistory &= Mask;

            // FIX: Update BTB
            long btbIndex = (long)((branchPC >> 1) & Mask);
            _tagBuffer[btbIndex] = branchPC;
            if (actuallyTaken) _targetBuffer[btbIndex] = actualTarget;
        }
    }
}
