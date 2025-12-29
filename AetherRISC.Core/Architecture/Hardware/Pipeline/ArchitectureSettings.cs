namespace AetherRISC.Core.Architecture.Hardware.Pipeline
{
    public class ArchitectureSettings
    {
        // If true: 1 Cycle Penalty (Fetch sees Execute update instantly).
        // If false: 2 Cycle Penalty (Fetch waits for next cycle).
        public bool EnableEarlyBranchResolution { get; set; } = true;

        // Starting value for predictors (0=StrongNT ... 3=StrongT for 2-bit)
        public int BranchPredictorInitialValue { get; set; } = 1;
    }
}
