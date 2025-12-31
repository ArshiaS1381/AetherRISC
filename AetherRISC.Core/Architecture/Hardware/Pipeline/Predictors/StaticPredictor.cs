using AetherRISC.Core.Abstractions.Interfaces;

namespace AetherRISC.Core.Architecture.Hardware.Pipeline.Predictors
{
    public class TunableStaticPredictor : IBranchPredictor
    {
        private readonly bool _predictTaken;
        public string Name => $"Static (Always {(_predictTaken ? "Taken" : "Not Taken")})";

        public TunableStaticPredictor(bool predictTaken)
        {
            _predictTaken = predictTaken;
        }

        public BranchPrediction Predict(ulong currentPC)
        {
            return new BranchPrediction { PredictedTaken = _predictTaken, TargetAddress = 0 };
        }

        public void Update(ulong branchPC, bool actuallyTaken, ulong actualTarget) { }
    }
}
