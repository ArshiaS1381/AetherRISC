using System.Collections.Generic;

namespace AetherRISC.Core.Architecture.Hardware.Pipeline
{
    public class ArchitectureSettings
    {
        public bool EnableEarlyBranchResolution { get; set; } = false;
        public int BranchPredictorInitialValue { get; set; } = 0;
        public int PipelineWidth { get; set; } = 1;
        public bool AllowDynamicBranchFetching { get; set; } = false;
        public bool AllowCascadedExecution { get; set; } = false;
        public bool EnableReturnAddressStack { get; set; } = false;
        public bool EnableMacroOpFusion { get; set; } = false;
        
        // --- Added for CLI Requirements ---
        public float FetchBufferRatio { get; set; } = 2.0f;
        
        // Resource Limits (0 = Infinite)
        public int MaxIntALUs { get; set; } = 0;
        public int MaxFloatALUs { get; set; } = 0;
        public int MaxMemoryUnits { get; set; } = 0;
        public int MaxBranchUnits { get; set; } = 0;

        public HashSet<string> DisabledInstructions { get; set; } = new HashSet<string>();
    }
}
