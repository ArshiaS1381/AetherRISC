using AetherRISC.Core.Abstractions.Interfaces;

namespace AetherRISC.Core.Architecture.Hardware.Pipeline.Predictors
{
    public class StaticPredictor : IBranchPredictor
    {
        public string Name => "Static (Always Not Taken)";

        public BranchPrediction Predict(ulong currentPC)
        {
            // Always predict fallthrough
            return new BranchPrediction { PredictedTaken = false, TargetAddress = 0 };
        }

        public void Update(ulong branchPC, bool actuallyTaken, ulong actualTarget)
        {
            // Static predictor has no state to update
        }
    }
}
