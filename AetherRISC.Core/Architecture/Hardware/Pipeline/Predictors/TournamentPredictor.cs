using AetherRISC.Core.Abstractions.Interfaces;
using System;

namespace AetherRISC.Core.Architecture.Hardware.Pipeline.Predictors
{
    public class TournamentPredictor : IBranchPredictor
    {
        public string Name => "Tournament (Bimodal vs GShare)";

        private readonly TunableBimodalPredictor _bimodal;
        private readonly TunableGSharePredictor _gshare;
        
        // Meta-predictor: 2-bit counters. 
        // 0,1 = prefer bimodal. 2,3 = prefer gshare.
        private readonly byte[] _meta; 
        private readonly int _mask;

        public TournamentPredictor(int bimodalBits, int gshareBits, int metaBits)
        {
            _bimodal = new TunableBimodalPredictor(bimodalBits, 2, 1);
            _gshare = new TunableGSharePredictor(12, gshareBits);
            
            int size = 1 << metaBits;
            _mask = size - 1;
            _meta = new byte[size];
            Array.Fill(_meta, (byte)1); // Weakly prefer bimodal init
        }

        public BranchPrediction Predict(ulong pc)
        {
            var pB = _bimodal.Predict(pc);
            var pG = _gshare.Predict(pc);
            
            uint idx = (uint)((pc >> 2) & (ulong)_mask);
            byte state = _meta[idx];

            return (state >= 2) ? pG : pB;
        }

        public void Update(ulong pc, bool taken, ulong target)
        {
            var pB = _bimodal.Predict(pc); // Re-predict to check accuracy
            var pG = _gshare.Predict(pc);
            
            bool bCorrect = pB.PredictedTaken == taken;
            bool gCorrect = pG.PredictedTaken == taken;

            uint idx = (uint)((pc >> 2) & (ulong)_mask);
            byte state = _meta[idx];

            // Update meta table if they differ
            if (bCorrect != gCorrect)
            {
                if (gCorrect) // GShare was right, Bimodal wrong -> increment
                {
                    if (state < 3) state++;
                }
                else // Bimodal was right -> decrement
                {
                    if (state > 0) state--;
                }
                _meta[idx] = state;
            }

            _bimodal.Update(pc, taken, target);
            _gshare.Update(pc, taken, target);
        }
    }
}
