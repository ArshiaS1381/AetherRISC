using AetherRISC.Core.Abstractions.Diagnostics;
using AetherRISC.Core.Abstractions.Interfaces;
using AetherRISC.Core.Architecture;
using AetherRISC.Core.Architecture.Hardware.Memory;
using System.Runtime.CompilerServices;

namespace AetherRISC.Core.Architecture.Hardware.Memory.Hierarchy
{
    public class CachedMemoryBus : IMemoryBus
    {
        private readonly IMemoryBus _phys;
        private readonly ArchitectureSettings _settings;
        private readonly PerformanceMetrics _metrics;

        // Controllers
        private readonly CacheController _l1I;
        private readonly CacheController _l1D;
        private readonly CacheController? _l2;
        private readonly CacheController? _l3;

        public CachedMemoryBus(IMemoryBus physical, ArchitectureSettings settings, PerformanceMetrics metrics)
        {
            _phys = physical;
            _settings = settings;
            _metrics = metrics;

            // Init L1
            _l1I = new CacheController("L1I", settings.L1ICacheSize, settings.L1ICacheWays, settings.L1ICacheLineSize, settings.L1ICacheLatency);
            _l1D = new CacheController("L1D", settings.L1DCacheSize, settings.L1DCacheWays, settings.L1DCacheLineSize, settings.L1DCacheLatency);

            // Init Optional L2
            if (settings.EnableL2Cache)
                _l2 = new CacheController("L2", settings.L2CacheSize, settings.L2CacheWays, 64, settings.L2CacheLatency);
            
            // Init Optional L3
            if (settings.EnableL3Cache)
                _l3 = new CacheController("L3", settings.L3CacheSize, settings.L3CacheWays, 64, settings.L3CacheLatency);
        }

        // --- IMemoryBus Passthrough ---
        public byte ReadByte(uint address) => _phys.ReadByte(address);
        public void WriteByte(uint address, byte value) => _phys.WriteByte(address, value);
        public ushort ReadHalf(uint address) => _phys.ReadHalf(address);
        public void WriteHalf(uint address, ushort value) => _phys.WriteHalf(address, value);
        public uint ReadWord(uint address) => _phys.ReadWord(address);
        public void WriteWord(uint address, uint value) => _phys.WriteWord(address, value);
        public ulong ReadDouble(uint address) => _phys.ReadDouble(address);
        public void WriteDouble(uint address, ulong value) => _phys.WriteDouble(address, value);

        // --- LATENCY SIMULATION ---
        // This is called by MemoryStage and FetchStage to determine stall cycles
        public int GetAccessLatency(uint address, bool isInstruction)
        {
            if (!_settings.EnableCacheSimulation) return 0;

            int totalLatency = 0;
            bool eviction;

            // 1. Check L1
            var l1 = isInstruction ? _l1I : _l1D;
            var l1Metric = isInstruction ? _metrics.L1I : _metrics.L1D;
            
            l1.Tick();
            totalLatency += l1.Latency;

            if (l1.Access(address, false, out eviction) == CacheAccessResult.Hit)
            {
                l1Metric.Hits++;
                return totalLatency;
            }
            
            l1Metric.Misses++;
            if(eviction) l1Metric.Evictions++;

            // 2. Check L2 (if enabled)
            if (_l2 != null)
            {
                _l2.Tick();
                totalLatency += _l2.Latency;
                if (_l2.Access(address, false, out eviction) == CacheAccessResult.Hit)
                {
                    _metrics.L2.Hits++;
                    return totalLatency;
                }
                _metrics.L2.Misses++;
                if(eviction) _metrics.L2.Evictions++;
            }

            // 3. Check L3 (if enabled)
            if (_l3 != null)
            {
                _l3.Tick();
                totalLatency += _l3.Latency;
                if (_l3.Access(address, false, out eviction) == CacheAccessResult.Hit)
                {
                    _metrics.L3.Hits++;
                    return totalLatency;
                }
                _metrics.L3.Misses++;
                if(eviction) _metrics.L3.Evictions++;
            }

            // 4. DRAM Access
            totalLatency += _settings.DramLatencyCycles;
            return totalLatency;
        }
    }
}
