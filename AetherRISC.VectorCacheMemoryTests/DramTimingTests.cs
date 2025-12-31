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
            if (field == null) throw new System.Exception("Field _dram not found");
            var val = field.GetValue(bus);
            return val as DramController ?? throw new System.Exception("Could not cast _dram");
        }

        [Fact]
        public void Bank_Parallelism_SeparateRowTracking()
        {
            var cfg = new ArchitectureSettings { EnableCacheSimulation = true };
            cfg.Dram.Banks = 4;
            cfg.Dram.RowSize = 1024;
            cfg.Dram.CAS = 10;
            cfg.Dram.RCD = 10;
            cfg.Dram.RP = 10;
            cfg.Dram.BurstLength = 0; 

            var ram = new PhysicalRam(0, 10000);
            var bus = new CachedMemoryBus(ram, cfg, new PerformanceMetrics());
            var dram = GetController(bus);

            int lat1 = dram.CalculateLatency(0, false, 0); 
            Assert.Equal(20, lat1);

            int lat2 = dram.CalculateLatency(1024, false, 0);
            Assert.Equal(20, lat2);

            int lat3 = dram.CalculateLatency(4, false, 0);
            Assert.Equal(10, lat3);

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

            dram.CalculateLatency(0, false, 0);
            int lat2 = dram.CalculateLatency(4, false, 0);
            
            Assert.Equal(20, lat2);
        }
    }
}
