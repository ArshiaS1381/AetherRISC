using System.Collections.Generic;

namespace AetherRISC.Core.Architecture
{
    public enum ReplacementPolicy { Random, LRU }
    public enum WritePolicy { WriteBack, WriteThrough }
    public enum AllocationPolicy { WriteAllocate, NoWriteAllocate }

    public class CacheConfiguration
    {
        public bool Enabled { get; set; } = true;
        public int SizeBytes { get; set; }
        public int Associativity { get; set; }
        public int LineSizeBytes { get; set; }
        public int LatencyCycles { get; set; }
        
        // Granular Tuning
        public ReplacementPolicy Replacement { get; set; } = ReplacementPolicy.LRU;
        public WritePolicy Write { get; set; } = WritePolicy.WriteBack;
        public AllocationPolicy Allocation { get; set; } = AllocationPolicy.WriteAllocate;

        public CacheConfiguration(int size, int ways, int lineSize, int latency)
        {
            SizeBytes = size;
            Associativity = ways;
            LineSizeBytes = lineSize;
            LatencyCycles = latency;
        }

        public CacheConfiguration() {}

        /// <summary>
        /// Helper to configure size based on geometry.
        /// </summary>
        public void ConfigureGeometry(int lines, int wordsPerLine, int wordSizeBytes = 4)
        {
            LineSizeBytes = wordsPerLine * wordSizeBytes;
            SizeBytes = lines * LineSizeBytes;
        }
    }

    public class ArchitectureSettings
    {
        public int XLEN { get; set; } = 64;

        // --- Pipeline Configuration ---
        public int PipelineWidth { get; set; } = 2;
        public double FetchBufferRatio { get; set; } = 2.0; 
        
        // --- Functional Unit Constraints (0 = Infinite) ---
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
        
        // Bimodal
        public int BimodalTableSizeBits { get; set; } = 12;
        public int BimodalCounterBits { get; set; } = 2;
        public int BimodalInitialValue { get; set; } = 1; // Fixed: Added missing property
        
        // GShare
        public int GShareHistoryBits { get; set; } = 12;
        public int GShareTableBits { get; set; } = 14;

        // --- Memory Hierarchy ---
        public bool EnableCacheSimulation { get; set; } = false;
        public uint MmioStartAddress { get; set; } = 0xF0000000; 

        // Detailed Cache Configs (Tunable per level)
        public CacheConfiguration L1I { get; set; } = new(32 * 1024, 4, 64, 1);
        public CacheConfiguration L1D { get; set; } = new(32 * 1024, 4, 64, 2);
        public CacheConfiguration L2 { get; set; } = new(256 * 1024, 8, 64, 10) { Enabled = false };
        public CacheConfiguration L3 { get; set; } = new(2 * 1024 * 1024, 16, 64, 30) { Enabled = false };

        // Main Memory
        public int DramLatencyCycles { get; set; } = 100;

        // --- RVV (Vectors) ---
        public bool EnableVectors { get; set; } = true;
        public int VectorLenBits { get; set; } = 128;
        public int VectorElen { get; set; } = 64;

        public List<string> DisabledInstructions { get; set; } = new();
    }
}
