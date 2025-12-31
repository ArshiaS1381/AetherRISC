using AetherRISC.Core.Abstractions.Diagnostics;
using AetherRISC.Core.Abstractions.Interfaces;
using AetherRISC.Core.Architecture;

namespace AetherRISC.Core.Architecture.Hardware.Memory.Hierarchy
{
    public class CachedMemoryBus : IMemoryBus
    {
        private readonly IMemoryBus _phys;
        private readonly ArchitectureSettings _settings;
        private readonly PerformanceMetrics _metrics;

        private readonly CacheController _l1I;
        private readonly CacheController _l1D;
        private readonly CacheController? _l2;
        private readonly CacheController? _l3;

        public CachedMemoryBus(IMemoryBus physical, ArchitectureSettings settings, PerformanceMetrics metrics)
        {
            _phys = physical;
            _settings = settings;
            _metrics = metrics;

            // Pass specific sub-configs
            _l1I = new CacheController("L1I", settings.L1I);
            _l1D = new CacheController("L1D", settings.L1D);

            if (settings.L2.Enabled)
                _l2 = new CacheController("L2", settings.L2);
            
            if (settings.L3.Enabled)
                _l3 = new CacheController("L3", settings.L3);
        }

        public byte ReadByte(uint a) => _phys.ReadByte(a);
        public void WriteByte(uint a, byte v) => _phys.WriteByte(a, v);
        public ushort ReadHalf(uint a) => _phys.ReadHalf(a);
        public void WriteHalf(uint a, ushort v) => _phys.WriteHalf(a, v);
        public uint ReadWord(uint a) => _phys.ReadWord(a);
        public void WriteWord(uint a, uint v) => _phys.WriteWord(a, v);
        public ulong ReadDouble(uint a) => _phys.ReadDouble(a);
        public void WriteDouble(uint a, ulong v) => _phys.WriteDouble(a, v);

        public int GetAccessLatency(uint address, bool isInstruction, bool isWrite)
        {
            if (!_settings.EnableCacheSimulation) return 0;

            // MMIO Bypass
            if (address >= _settings.MmioStartAddress) return _settings.DramLatencyCycles;

            int latency = 0;
            bool wb;
            
            // --- L1 ---
            var l1 = isInstruction ? _l1I : _l1D;
            var l1M = isInstruction ? _metrics.L1I : _metrics.L1D;
            l1.Tick();
            latency += l1.Latency;
            
            var res = l1.Access(address, isWrite, out wb);
            if (res == CacheAccessResult.Hit)
            {
                l1M.Hits++;
                if (wb && _l2 != null) latency += _l2.Latency; 
                else if (wb) latency += _settings.DramLatencyCycles;
                return latency;
            }
            l1M.Misses++;
            if(wb) l1M.Evictions++;

            // --- L2 ---
            if (_l2 != null)
            {
                _l2.Tick();
                latency += _l2.Latency;
                res = _l2.Access(address, isWrite, out wb); 
                if (res == CacheAccessResult.Hit)
                {
                    _metrics.L2.Hits++;
                    if (wb) latency += _l3?.Latency ?? _settings.DramLatencyCycles;
                    return latency;
                }
                _metrics.L2.Misses++;
                if(wb) _metrics.L2.Evictions++;
            }

            // --- L3 ---
            if (_l3 != null)
            {
                _l3.Tick();
                latency += _l3.Latency;
                res = _l3.Access(address, isWrite, out wb);
                if (res == CacheAccessResult.Hit)
                {
                    _metrics.L3.Hits++;
                    if(wb) latency += _settings.DramLatencyCycles;
                    return latency;
                }
                _metrics.L3.Misses++;
                if(wb) _metrics.L3.Evictions++;
            }

            // --- DRAM ---
            latency += _settings.DramLatencyCycles;
            return latency;
        }
        
        public int GetAccessLatency(uint address, bool isInstruction) => GetAccessLatency(address, isInstruction, false);
    }
}
