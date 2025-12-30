using AetherRISC.Core.Abstractions.Interfaces;
using System;
using System.Runtime.CompilerServices;

namespace AetherRISC.Core.Architecture.Hardware.Pipeline.Predictors
{
    public unsafe class GsharePredictor : IBranchPredictor
    {
        public string Name => "Gshare (Global History)";

        private const int HistoryBits = 12; 
        private const int TableSize = 1 << HistoryBits;
        private const int Mask = TableSize - 1;

        private readonly byte[] _pht = new byte[TableSize];
        private readonly ulong[] _targetBuffer = new ulong[TableSize];
        private readonly ulong[] _tagBuffer = new ulong[TableSize];
        
        private uint _globalHistory = 0;

        public GsharePredictor()
        {
            Array.Fill(_pht, (byte)1); 
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BranchPrediction Predict(ulong currentPC)
        {
            uint pcPart = (uint)((currentPC >> 1) & Mask);
            uint index = pcPart ^ _globalHistory;
            
            bool predictTaken;
            ulong target = 0;

            fixed (byte* phtPtr = _pht)
            fixed (ulong* targetPtr = _targetBuffer)
            fixed (ulong* tagPtr = _tagBuffer)
            {
                byte state = phtPtr[index];
                predictTaken = state >= 2;

                if (tagPtr[pcPart] == currentPC)
                {
                    target = targetPtr[pcPart];
                }
            }

            if (target == 0) predictTaken = false;

            return new BranchPrediction 
            { 
                PredictedTaken = predictTaken, 
                TargetAddress = target 
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Update(ulong branchPC, bool actuallyTaken, ulong actualTarget)
        {
            uint pcPart = (uint)((branchPC >> 1) & Mask);
            uint index = pcPart ^ _globalHistory;

            fixed (byte* phtPtr = _pht)
            {
                byte state = phtPtr[index];
                if (actuallyTaken) { if (state < 3) state++; }
                else { if (state > 0) state--; }
                phtPtr[index] = state;
            }

            _globalHistory = (_globalHistory << 1) | (actuallyTaken ? 1u : 0u);
            _globalHistory &= Mask;

            fixed (ulong* tagPtr = _tagBuffer)
            fixed (ulong* targetPtr = _targetBuffer)
            {
                tagPtr[pcPart] = branchPC;
                if (actuallyTaken) targetPtr[pcPart] = actualTarget;
            }
        }
    }
}
