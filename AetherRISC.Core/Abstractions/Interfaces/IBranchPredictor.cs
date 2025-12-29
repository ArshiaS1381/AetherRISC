namespace AetherRISC.Core.Abstractions.Interfaces
{
    public struct BranchPrediction
    {
        public bool PredictedTaken;
        public ulong TargetAddress;
    }

    public interface IBranchPredictor
    {
        string Name { get; }
        
        /// <summary>
        /// Predicts the next Program Counter based on the current PC.
        /// </summary>
        BranchPrediction Predict(ulong currentPC);

        /// <summary>
        /// Updates the internal history/state of the predictor based on actual results.
        /// </summary>
        void Update(ulong branchPC, bool actuallyTaken, ulong actualTarget);
    }
}
