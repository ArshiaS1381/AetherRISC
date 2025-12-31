using Xunit;
using AetherRISC.Core.Architecture;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.Memory.Hierarchy;
using AetherRISC.Core.Architecture.Memory.Physical;
using AetherRISC.Core.Abstractions.Diagnostics;
using System.Linq;

namespace AetherRISC.VectorCacheMemoryTests
{
    public class CacheFunctionalTests
    {
        [Fact]
        public void Associativity_Eviction_WorksCorrectly()
        {
            var cfg = new ArchitectureSettings { EnableCacheSimulation = true };
            cfg.L1D.ConfigureGeometry(lines: 2, wordsPerLine: 16); 
            cfg.L1D.Associativity = 2; 
            cfg.L1D.Replacement = ReplacementPolicy.LRU;

            var metrics = new PerformanceMetrics();
            var ram = new PhysicalRam(0, 4096);
            var bus = new CachedMemoryBus(ram, cfg, metrics);

            bus.WriteWord(0x1000, 0xAAAA); 
            bus.WriteWord(0x2000, 0xBBBB); 
            bus.ReadWord(0x1000); 
            bus.WriteWord(0x3000, 0xCCCC); 

            metrics.L1D.Hits = 0;
            metrics.L1D.Misses = 0;

            bus.ReadWord(0x1000); 
            bus.ReadWord(0x2000); 

            Assert.Equal(1u, metrics.L1D.Hits); 
            Assert.Equal(1u, metrics.L1D.Misses); 
        }

        [Fact]
        public void Replacement_Random_IsActuallyRandom()
        {
            var cfg = new ArchitectureSettings { EnableCacheSimulation = true };
            cfg.L1D.ConfigureGeometry(lines: 2, wordsPerLine: 16);
            cfg.L1D.Associativity = 2;
            cfg.L1D.Replacement = ReplacementPolicy.Random;

            var ram = new PhysicalRam(0, 4096);
            int evictA = 0;
            int evictB = 0;
            
            for(int i=0; i<50; i++)
            {
                uint addrA = 0x1000;
                uint addrB = 0x2000;
                uint addrC = 0x3000;

                var loopMetrics = new PerformanceMetrics();
                var loopBus = new CachedMemoryBus(ram, cfg, loopMetrics);
                
                loopBus.WriteWord(addrA, 1);
                loopBus.WriteWord(addrB, 2);
                loopBus.WriteWord(addrC, 3);
                
                ulong missStart = loopMetrics.L1D.Misses;
                loopBus.ReadWord(addrA);
                if (loopMetrics.L1D.Misses > missStart) evictA++;

                missStart = loopMetrics.L1D.Misses;
                loopBus.ReadWord(addrB);
                if (loopMetrics.L1D.Misses > missStart) evictB++;
            }

            Assert.True(evictA > 0, "Random policy never evicted Way 0");
            Assert.True(evictB > 0, "Random policy never evicted Way 1");
        }

        [Fact]
        public void WriteBack_Vs_WriteThrough_RAM()
        {
            var cfgWB = new ArchitectureSettings { EnableCacheSimulation = true };
            cfgWB.L1D.Write = WritePolicy.WriteBack;
            
            var cfgWT = new ArchitectureSettings { EnableCacheSimulation = true };
            cfgWT.L1D.Write = WritePolicy.WriteThrough;

            var ram = new PhysicalRam(0, 1024);
            var busWB = new CachedMemoryBus(ram, cfgWB, new PerformanceMetrics());
            var busWT = new CachedMemoryBus(ram, cfgWT, new PerformanceMetrics());

            busWB.WriteWord(0x10, 0xAAAA);
            Assert.Equal(0u, ram.ReadWord(0x10)); 

            busWT.WriteWord(0x20, 0xBBBB);
            Assert.Equal(0xBBBBu, ram.ReadWord(0x20)); 
        }
    }
}
