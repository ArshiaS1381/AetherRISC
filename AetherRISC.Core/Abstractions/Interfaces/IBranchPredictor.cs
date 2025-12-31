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
        
        BranchPrediction Predict(ulong currentPC);

        void Update(ulong branchPC, bool actuallyTaken, ulong actualTarget);
    }
}
