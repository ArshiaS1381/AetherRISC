using Xunit;
using AetherRISC.Core.Architecture;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.Memory;
using AetherRISC.Core.Architecture.Hardware.Memory.Hierarchy;
using AetherRISC.Core.Architecture.Memory.Physical;
using AetherRISC.Core.Abstractions.Diagnostics;

namespace AetherRISC.VectorCacheMemoryTests
{
    public class CachePolicyTests
    {
        [Fact]
        public void WriteBack_Policy_DelaysMemoryWrite()
        {
            var metrics = new PerformanceMetrics();
            var cfg = new ArchitectureSettings 
            { 
                EnableCacheSimulation = true
            };
            
            // Configure L1D specific settings
            cfg.L1D.Write = WritePolicy.WriteBack;
            cfg.L1D.SizeBytes = 256;
            cfg.L1D.LineSizeBytes = 64;
            cfg.L1D.Associativity = 1;
            
            var ram = new PhysicalRam(0, 1024);
            var bus = new CachedMemoryBus(ram, cfg, metrics);

            bus.WriteWord(0x100, 0xDEADBEEF);

            // RAM should NOT have value yet (Write-Back)
            Assert.Equal(0u, ram.ReadWord(0x100));

            // Force eviction (0x100 -> Set 0. 0x200 -> Set 0)
            bus.WriteWord(0x200, 0xCAFEBABE);

            // Now 0x100 should be evicted and written back to RAM
            Assert.Equal(0xDEADBEEF, ram.ReadWord(0x100));
        }

        [Fact]
        public void WriteThrough_Policy_WritesImmediately()
        {
            var metrics = new PerformanceMetrics();
            var cfg = new ArchitectureSettings 
            { 
                EnableCacheSimulation = true
            };
            cfg.L1D.Write = WritePolicy.WriteThrough;
            
            var ram = new PhysicalRam(0, 1024);
            var bus = new CachedMemoryBus(ram, cfg, metrics);

            bus.WriteWord(0x100, 0x12345678);

            // RAM SHOULD have value immediately
            Assert.Equal(0x12345678, ram.ReadWord(0x100));
        }

        [Fact]
        public void MMIO_BypassesCache()
        {
            var metrics = new PerformanceMetrics();
            var cfg = new ArchitectureSettings 
            { 
                EnableCacheSimulation = true,
                MmioStartAddress = 0xF000
            };
            
            var ram = new PhysicalRam(0, 0xFFFF);
            var bus = new CachedMemoryBus(ram, cfg, metrics);

            // Access below MMIO -> Cache Latency (~2)
            int latNormal = bus.GetAccessLatency(0x100, false, false);
            Assert.True(latNormal < 100); 

            // Access MMIO -> DRAM Latency (100)
            int latMmio = bus.GetAccessLatency(0xF004, false, false);
            Assert.Equal(cfg.DramLatencyCycles, latMmio);
        }
    }
}
