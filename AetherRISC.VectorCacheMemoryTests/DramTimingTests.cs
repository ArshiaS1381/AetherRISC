using Xunit;
using AetherRISC.Core.Architecture;
using AetherRISC.Core.Architecture.Hardware.Memory.Hierarchy;
using AetherRISC.Core.Architecture.Memory.Physical;
using AetherRISC.Core.Abstractions.Diagnostics;
using System.Reflection;

namespace AetherRISC.VectorCacheMemoryTests
{
    public class DramTimingTests
    {
        private DramController GetController(CachedMemoryBus bus)
        {
            var field = typeof(CachedMemoryBus).GetField("_dram", BindingFlags.NonPublic | BindingFlags.Instance);
            return (DramController)field.GetValue(bus);
        }

        [Fact]
        public void Bank_Parallelism_SeparateRowTracking()
        {
            var cfg = new ArchitectureSettings { EnableCacheSimulation = true };
            cfg.Dram.Banks = 4;
            cfg.Dram.RowSize = 1024;
            // Timings
            cfg.Dram.CAS = 10;
            cfg.Dram.RCD = 10;
            cfg.Dram.RP = 10;
            cfg.Dram.BurstLength = 0; // simplify calc

            var ram = new PhysicalRam(0, 10000);
            var bus = new CachedMemoryBus(ram, cfg, new PerformanceMetrics());
            var dram = GetController(bus);

            // Address Map: 
            // 0 -> Bank 0, Row 0
            // 1024 -> Bank 1, Row 0 (Interleaved pages)
            
            // 1. Open Bank 0, Row 0. Cost: RCD+CAS = 20.
            int lat1 = dram.CalculateLatency(0, false, 0); 
            Assert.Equal(20, lat1);

            // 2. Open Bank 1, Row 0. Cost: RCD+CAS = 20 (Should NOT conflict with Bank 0).
            int lat2 = dram.CalculateLatency(1024, false, 0);
            Assert.Equal(20, lat2);

            // 3. Access Bank 0, Row 0 again. Cost: CAS = 10 (Row Hit).
            int lat3 = dram.CalculateLatency(4, false, 0);
            Assert.Equal(10, lat3);

            // 4. Access Bank 0, Row 1 (Conflict).
            // Cost: RP (Precharge Row 0) + RCD (Activate Row 1) + CAS = 30.
            // Row 1 starts at: Bank 0 index wraps every 4 pages.
            // Page 0 (B0), Page 1 (B1), Page 2 (B2), Page 3 (B3), Page 4 (B0, Row 1).
            // Addr = 4 * 1024 = 4096.
            int lat4 = dram.CalculateLatency(4096, false, 0);
            Assert.Equal(30, lat4);
        }

        [Fact]
        public void ClosePage_Policy_AlwaysPrecharges()
        {
            var cfg = new ArchitectureSettings { EnableCacheSimulation = true };
            cfg.Dram.PagePolicy = DramPagePolicy.ClosePage;
            cfg.Dram.CAS = 10;
            cfg.Dram.RCD = 10;
            cfg.Dram.RP = 10;

            var ram = new PhysicalRam(0, 10000);
            var bus = new CachedMemoryBus(ram, cfg, new PerformanceMetrics());
            var dram = GetController(bus);

            // 1. Access 0x0. Closed -> Open -> Access -> Close.
            // Latency returned is access time (RCD+CAS = 20). 
            // Controller auto-closes row logically.
            dram.CalculateLatency(0, false, 0);

            // 2. Access 0x4 (Same row). 
            // Since it auto-closed, this looks like a fresh activation, NOT a Row Hit.
            // Latency: RCD+CAS = 20. (If OpenPage, would be 10).
            int lat2 = dram.CalculateLatency(4, false, 0);
            
            Assert.Equal(20, lat2);
        }
    }
}
