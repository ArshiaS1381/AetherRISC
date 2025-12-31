namespace AetherRISC.Core.Abstractions.Diagnostics
{
    public class PerformanceMetrics
    {
        public int PipelineWidth { get; set; }
        public ulong TotalCycles { get; set; }
        public ulong InstructionsRetired { get; set; }
        public ulong IsaInstructionsRetired { get; set; }
        
        // Pipeline Stats
        public ulong DataHazardStalls { get; set; }
        public ulong ControlHazardFlushes { get; set; }
        public ulong TotalBranches { get; set; }
        public ulong BranchHits { get; set; }
        public ulong BranchMisses { get; set; }

        // Cache Stats
        public CacheMetric L1I { get; } = new();
        public CacheMetric L1D { get; } = new();
        public CacheMetric L2 { get; } = new();
        public CacheMetric L3 { get; } = new();

        public class CacheMetric
        {
            public ulong Hits { get; set; }
            public ulong Misses { get; set; }
            public ulong Evictions { get; set; }
            public double HitRate => (Hits + Misses) == 0 ? 0 : (double)Hits / (Hits + Misses) * 100.0;
        }

        public double Ipc => TotalCycles == 0 ? 0 : (double)InstructionsRetired / TotalCycles;
    }
}
