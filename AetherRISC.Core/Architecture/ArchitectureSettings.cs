using System.Collections.Generic;

namespace AetherRISC.Core.Architecture
{
    public enum ReplacementPolicy { Random, LRU }
    public enum WritePolicy { WriteBack, WriteThrough }
    public enum AllocationPolicy { WriteAllocate, NoWriteAllocate }
    public enum DramPagePolicy { OpenPage, ClosePage }
    public enum DramGeneration { Custom, DDR3, DDR4, DDR5, LPDDR4, LPDDR5, HBM2 }

    public class CacheConfiguration
    {
        public bool Enabled { get; set; } = true;
        public int SizeBytes { get; set; }
        public int Associativity { get; set; }
        public int LineSizeBytes { get; set; }
        public int LatencyCycles { get; set; }
        
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

        public void ConfigureGeometry(int lines, int wordsPerLine, int wordSizeBytes = 4)
        {
            LineSizeBytes = wordsPerLine * wordSizeBytes;
            SizeBytes = lines * LineSizeBytes;
        }
    }

    public class DramConfiguration
    {
        public DramGeneration Generation { get; set; } = DramGeneration.DDR4;
        
        public int CAS { get; set; } = 14;         
        public int RCD { get; set; } = 14;         
        public int RP { get; set; } = 14;          
        public int RAS { get; set; } = 34;         
        public int WR { get; set; } = 16;          
        
        public int BurstLength { get; set; } = 8;
        public bool BurstChop { get; set; } = false;
        public int BusWidthBits { get; set; } = 64; 
        
        public int Banks { get; set; } = 16;
        public int BankGroups { get; set; } = 4;
        public int RowSize { get; set; } = 8192; 
        public DramPagePolicy PagePolicy { get; set; } = DramPagePolicy.OpenPage;
        
        public int FixedLatency { get; set; } = 0;

        public void ApplyPreset(DramGeneration gen)
        {
            Generation = gen;
            switch(gen)
            {
                case DramGeneration.DDR3:
                    CAS=11; RCD=11; RP=11; RAS=28; 
                    Banks=8; BankGroups=1; BurstLength=8; BusWidthBits=64;
                    break;
                case DramGeneration.DDR4:
                    CAS=22; RCD=22; RP=22; RAS=52; 
                    Banks=16; BankGroups=4; BurstLength=8; BusWidthBits=64;
                    break;
                case DramGeneration.DDR5:
                    CAS=40; RCD=39; RP=39; RAS=70;
                    Banks=32; BankGroups=8; BurstLength=16; BusWidthBits=32; 
                    break;
                case DramGeneration.HBM2:
                    CAS=14; RCD=14; RP=14; RAS=33;
                    Banks=64; BurstLength=4; BusWidthBits=1024; 
                    break;
            }
        }
    }

    public class ArchitectureSettings
    {
        public int XLEN { get; set; } = 64;

        // --- Pipeline: Defaults to 1-wide Scalar ---
        public int PipelineWidth { get; set; } = 1;
        public double FetchBufferRatio { get; set; } = 1.0; 
        public int BranchFlushPenalty { get; set; } = 2;

        // --- Execution Units: Default to Single Units ---
        public int MaxIntALUs { get; set; } = 1;
        public int MaxFloatALUs { get; set; } = 1;
        public int MaxMemoryUnits { get; set; } = 1;
        public int MaxBranchUnits { get; set; } = 1;
        public int MaxVectorUnits { get; set; } = 1;

        // --- Optimization Flags: Disabled by Default ---
        public bool EnableMacroOpFusion { get; set; } = false;
        public bool AllowCascadedExecution { get; set; } = false;
        public bool EnableReturnAddressStack { get; set; } = false;
        public int RasSize { get; set; } = 16;
        
        // --- Fetch: Simple Word Fetch ---
        public bool EnableBlockFetching { get; set; } = false; 
        public int FetchBlockSize { get; set; } = 4; // Word size
        public bool EnableDynamicBranchFetching { get; set; } = false; 

        // --- Prediction: Static ---
        public string BranchPredictorType { get; set; } = "static"; 
        public bool StaticPredictTaken { get; set; } = false;
        
        public int BimodalTableSizeBits { get; set; } = 12;
        public int BimodalCounterBits { get; set; } = 2;
        public int BimodalInitialValue { get; set; } = 1;
        public int GShareHistoryBits { get; set; } = 12;
        public int GShareTableBits { get; set; } = 14;

        // --- Memory Hierarchy: Disabled ---
        public bool EnableCacheSimulation { get; set; } = false;
        public uint MmioStartAddress { get; set; } = 0xF0000000; 

        // Defaults (Ignored unless EnableCacheSimulation is true)
        public CacheConfiguration L1I { get; set; } = new(32 * 1024, 4, 64, 1);
        public CacheConfiguration L1D { get; set; } = new(32 * 1024, 4, 64, 2);
        public CacheConfiguration L2 { get; set; } = new(256 * 1024, 8, 64, 10) { Enabled = false };
        public CacheConfiguration L3 { get; set; } = new(2 * 1024 * 1024, 16, 64, 30) { Enabled = false };

        public DramConfiguration Dram { get; set; } = new();

        // --- RVV: Disabled ---
        public bool EnableVectors { get; set; } = false;
        public int VectorLenBits { get; set; } = 128;
        public int VectorElen { get; set; } = 64;

        public List<string> DisabledInstructions { get; set; } = new();
    }
}
