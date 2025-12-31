namespace AetherRISC.Core.Abstractions.Diagnostics
{
    public class PerformanceMetrics
    {

        public ulong InstructionsRetired { get; set; }
        
        // ISA-level instructions (Effective instructions executed, e.g. a Fused op counts as 2)
        public ulong IsaInstructionsRetired { get; set; }

        public ulong TotalCycles { get; set; }
        public ulong TotalBranches { get; set; }
        public ulong BranchMisses { get; set; }
        public ulong BranchHits { get; set; }
        public ulong ControlHazardFlushes { get; set; }
        public ulong DataHazardStalls { get; set; }
        public int PipelineWidth { get; set; }

        // Pipeline Throughput (Commits per cycle)
        public double PipelineIPC => TotalCycles == 0 ? 0 : (double)InstructionsRetired / TotalCycles;
        
        // Effective Throughput (ISA work per cycle - usually higher if fusion is on)
        public double EffectiveIPC => TotalCycles == 0 ? 0 : (double)IsaInstructionsRetired / TotalCycles;

        public double BranchAccuracy => TotalBranches == 0 ? 100.0 : (double)BranchHits / TotalBranches * 100.0;
        
        public double SlotUtilization => (TotalCycles * (ulong)PipelineWidth) == 0 
            ? 0 
            : (double)InstructionsRetired / (double)(TotalCycles * (ulong)PipelineWidth) * 100.0;
    }
}
