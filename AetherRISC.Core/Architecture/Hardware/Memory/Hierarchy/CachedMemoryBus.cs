using AetherRISC.Core.Abstractions.Diagnostics;
using AetherRISC.Core.Abstractions.Interfaces;
using AetherRISC.Core.Architecture;
using System;

namespace AetherRISC.Core.Architecture.Hardware.Memory.Hierarchy
{
    public class CachedMemoryBus : IMemoryBus
    {
        private readonly IMemoryBus _phys;
        private readonly ArchitectureSettings _settings;
        private readonly PerformanceMetrics _metrics;
        private readonly DramController _dram;

        private readonly CacheController _l1I;
        private readonly CacheController _l1D;
        
        public CachedMemoryBus(IMemoryBus physical, ArchitectureSettings settings, PerformanceMetrics metrics)
        {
            _phys = physical;
            _settings = settings;
            _metrics = metrics;
            _dram = new DramController(settings.Dram);

            _l1I = new CacheController("L1I", settings.L1I);
            _l1D = new CacheController("L1D", settings.L1D);
        }

        public byte ReadByte(uint a) => ReadInternal(a, false); 
        public void WriteByte(uint a, byte v) => WriteInternal(a, v);
        public ushort ReadHalf(uint a) => (ushort)(ReadByte(a) | (ReadByte(a + 1) << 8));
        public void WriteHalf(uint a, ushort v) { WriteByte(a, (byte)v); WriteByte(a+1, (byte)(v>>8)); }
        public uint ReadWord(uint a) => ReadInternalWord(a, false);
        public void WriteWord(uint a, uint v) => WriteInternalWord(a, v);
        public ulong ReadDouble(uint a) => (ulong)ReadWord(a) | ((ulong)ReadWord(a + 4) << 32);
        public void WriteDouble(uint a, ulong v) { WriteWord(a, (uint)v); WriteWord(a+4, (uint)(v>>32)); }

        // --- Byte Logic ---
        private byte ReadInternal(uint address, bool isInst)
        {
            if (!_settings.EnableCacheSimulation || address >= _settings.MmioStartAddress) return _phys.ReadByte(address);

            var cache = isInst ? _l1I : _l1D;
            var metric = isInst ? _metrics.L1I : _metrics.L1D;
            cache.Tick();

            if (cache.TryRead(address, out byte val))
            {
                metric.Hits++;
                return val;
            }

            metric.Misses++;
            HandleMiss(cache, metric, address);
            return _phys.ReadByte(address);
        }

        private void WriteInternal(uint address, byte val)
        {
            if (!_settings.EnableCacheSimulation || address >= _settings.MmioStartAddress) 
            {
                _phys.WriteByte(address, val);
                return;
            }

            var cache = _l1D;
            var metric = _metrics.L1D;
            cache.Tick();

            if (cache.TryWrite(address, val))
            {
                metric.Hits++;
                if (cache.WritePolicy == WritePolicy.WriteThrough) _phys.WriteByte(address, val);
                return;
            }

            metric.Misses++;
            if (cache.AllocPolicy == AllocationPolicy.NoWriteAllocate)
            {
                _phys.WriteByte(address, val);
            }
            else
            {
                HandleMiss(cache, metric, address);
                cache.TryWrite(address, val);
                if (cache.WritePolicy == WritePolicy.WriteThrough) _phys.WriteByte(address, val);
            }
        }

        // --- Word Logic ---
        private uint ReadInternalWord(uint address, bool isInst)
        {
            if (!_settings.EnableCacheSimulation || address >= _settings.MmioStartAddress) return _phys.ReadWord(address);

            var cache = isInst ? _l1I : _l1D;
            var metric = isInst ? _metrics.L1I : _metrics.L1D;
            cache.Tick();

            if (cache.TryReadWord(address, out uint val))
            {
                metric.Hits++;
                return val;
            }

            metric.Misses++;
            HandleMiss(cache, metric, address);
            return _phys.ReadWord(address);
        }

        private void WriteInternalWord(uint address, uint val)
        {
            if (!_settings.EnableCacheSimulation || address >= _settings.MmioStartAddress)
            {
                _phys.WriteWord(address, val);
                return;
            }

            var cache = _l1D;
            var metric = _metrics.L1D;
            cache.Tick();

            if (cache.TryWriteWord(address, val))
            {
                metric.Hits++;
                if (cache.WritePolicy == WritePolicy.WriteThrough) _phys.WriteWord(address, val);
                return;
            }

            metric.Misses++;
            if (cache.AllocPolicy == AllocationPolicy.NoWriteAllocate)
            {
                _phys.WriteWord(address, val);
            }
            else
            {
                HandleMiss(cache, metric, address);
                cache.TryWriteWord(address, val);
                if (cache.WritePolicy == WritePolicy.WriteThrough) _phys.WriteWord(address, val);
            }
        }

        // Removed the internal metric.Misses++ to prevent double counting
        private void HandleMiss(CacheController cache, PerformanceMetrics.CacheMetric metric, uint address)
        {
            // Note: Misses++ is handled by caller now
            int blockSize = _settings.L1D.LineSizeBytes;
            uint baseAddr = address & ~((uint)blockSize - 1);
            
            byte[] block = new byte[blockSize];
            for(int i=0; i<blockSize; i++) block[i] = _phys.ReadByte(baseAddr + (uint)i);

            cache.Fill(baseAddr, block, out ulong? evAddr, out byte[]? evData);
            
            if (evAddr.HasValue && evData != null)
            {
                metric.Evictions++;
                for(int i=0; i<evData.Length; i++) 
                    _phys.WriteByte((uint)evAddr.Value + (uint)i, evData[i]);
            }
        }
        
        public int GetAccessLatency(uint address, bool isInstruction, bool isWrite)
        {
             if (!_settings.EnableCacheSimulation) return 0;
             if (address >= _settings.MmioStartAddress) return _settings.Dram.FixedLatency > 0 ? _settings.Dram.FixedLatency : 100;
             int lat = isInstruction ? _l1I.Latency : _l1D.Latency;
             // Add DRAM latency to simulate miss cost overheads on memory requests
             // Note: In functional sim, we add this calculation for "accuracy" of stall time, 
             // even though the data is already fetched.
             return lat + _dram.CalculateLatency(address, isWrite, 64); // Assume 64-byte burst for cache line
        }
        
        public int GetAccessLatency(uint address, bool isInstruction) => GetAccessLatency(address, isInstruction, false);
    }
}
