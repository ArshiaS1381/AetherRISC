using System.Collections.Generic;

namespace AetherRISC.Core.Architecture
{
    public class ArchitectureSettings
    {
        // Global Architecture
        public int XLEN { get; set; } = 64;

        // --- Pipeline Configuration ---
        public int PipelineWidth { get; set; } = 2;
        public double FetchBufferRatio { get; set; } = 2.0; 
        
        // --- Functional Unit Constraints ---
        public int MaxIntALUs { get; set; } = 2;
        public int MaxFloatALUs { get; set; } = 1;
        public int MaxMemoryUnits { get; set; } = 1;
        public int MaxBranchUnits { get; set; } = 1;
        public int MaxVectorUnits { get; set; } = 1;

        // --- Optimization Flags ---
        public bool EnableMacroOpFusion { get; set; } = true;
        public bool AllowCascadedExecution { get; set; } = true;
        public bool EnableReturnAddressStack { get; set; } = true;
        public int RasSize { get; set; } = 16;
        
        // --- Fetch Configuration ---
        public bool EnableBlockFetching { get; set; } = true; 
        public int FetchBlockSize { get; set; } = 16;
        public bool EnableDynamicBranchFetching { get; set; } = true; 

        // --- Branch Prediction ---
        public string BranchPredictorType { get; set; } = "gshare"; 
        public bool StaticPredictTaken { get; set; } = false;
        public int BimodalTableSizeBits { get; set; } = 12;
        public int BimodalCounterBits { get; set; } = 2;
        public int BimodalInitialValue { get; set; } = 1;
        public int GShareHistoryBits { get; set; } = 12;
        public int GShareTableBits { get; set; } = 14;

        // --- Memory Hierarchy (Cache Simulation) ---
        public bool EnableCacheSimulation { get; set; } = false;
        
        // L1 Cache
        public int L1ICacheSize { get; set; } = 32 * 1024;
        public int L1ICacheWays { get; set; } = 4;
        public int L1ICacheLineSize { get; set; } = 64; // Added
        public int L1ICacheLatency { get; set; } = 1;

        public int L1DCacheSize { get; set; } = 32 * 1024;
        public int L1DCacheWays { get; set; } = 4;
        public int L1DCacheLineSize { get; set; } = 64; // Added
        public int L1DCacheLatency { get; set; } = 2;

        // L2 Cache
        public bool EnableL2Cache { get; set; } = false;
        public int L2CacheSize { get; set; } = 256 * 1024; 
        public int L2CacheWays { get; set; } = 8;
        public int L2CacheLatency { get; set; } = 10;

        // L3 Cache
        public bool EnableL3Cache { get; set; } = false;
        public int L3CacheSize { get; set; } = 2 * 1024 * 1024;
        public int L3CacheWays { get; set; } = 16;
        public int L3CacheLatency { get; set; } = 30;

        // Main Memory
        public int DramLatencyCycles { get; set; } = 100;

        // --- RVV (Vectors) ---
        public bool EnableVectors { get; set; } = true;
        public int VectorLenBits { get; set; } = 128;
        public int VectorElen { get; set; } = 64;

        public List<string> DisabledInstructions { get; set; } = new();
    }
}
