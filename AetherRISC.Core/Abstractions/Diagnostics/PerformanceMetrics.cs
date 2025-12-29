namespace AetherRISC.Core.Abstractions.Diagnostics
{
    public class PerformanceMetrics
    {
        // --- Throughput ---
        public long TotalCycles { get; set; }
        public long InstructionsRetired { get; set; }
        
        // IPC: Higher is better (Target: 1.0)
        public double IPC => TotalCycles == 0 ? 0 : (double)InstructionsRetired / TotalCycles;
        
        // CPI: Lower is better (Target: 1.0)
        public double CPI => InstructionsRetired == 0 ? 0 : (double)TotalCycles / InstructionsRetired;

        // --- Branch Prediction ---
        public long TotalBranches { get; set; }
        public long BranchHits { get; set; }
        public long BranchMisses { get; set; }
        public long TotalJumps { get; set; }

        public double BranchAccuracy => TotalBranches == 0 ? 0 : (double)BranchHits / TotalBranches * 100.0;
        public double MispredictionRate => TotalBranches == 0 ? 0 : (double)BranchMisses / TotalBranches * 100.0;

        // --- Pipeline Health ---
        public long ControlHazardFlushes { get; set; }
        public long DataHazardStalls { get; set; }

        // Efficiency: What % of cycles were NOT wasted?
        public double PipelineEfficiency 
        {
            get 
            {
                if (TotalCycles == 0) return 0;
                long wasted = ControlHazardFlushes + DataHazardStalls;
                return 100.0 * (1.0 - ((double)wasted / TotalCycles));
            }
        }
    }
}
