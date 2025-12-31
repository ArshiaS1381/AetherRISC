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

        private readonly CacheController _l1I;
        private readonly CacheController _l1D;
        
        // Simulating a unified lower level memory (L2/Main) for simplicity in this step, 
        // effectively L1 on top of RAM. Extending to full L2/L3 in functional mode requires recursive logic.
        // For now, backing store is _phys.

        public CachedMemoryBus(IMemoryBus physical, ArchitectureSettings settings, PerformanceMetrics metrics)
        {
            _phys = physical;
            _settings = settings;
            _metrics = metrics;

            _l1I = new CacheController("L1I", settings.L1I);
            _l1D = new CacheController("L1D", settings.L1D);
        }

        // --- Core Access Methods ---

        public byte ReadByte(uint address) => ReadInternal(address, false); // Data access
        public void WriteByte(uint address, byte value) => WriteInternal(address, value);

        public ushort ReadHalf(uint a) => (ushort)(ReadByte(a) | (ReadByte(a + 1) << 8));
        public void WriteHalf(uint a, ushort v) { WriteByte(a, (byte)v); WriteByte(a+1, (byte)(v>>8)); }

        public uint ReadWord(uint a) => (uint)(ReadHalf(a) | (ReadHalf(a + 2) << 16));
        public void WriteWord(uint a, uint v) { WriteHalf(a, (ushort)v); WriteHalf(a+2, (ushort)(v>>16)); }

        public ulong ReadDouble(uint a) => (ulong)ReadWord(a) | ((ulong)ReadWord(a + 4) << 32);
        public void WriteDouble(uint a, ulong v) { WriteWord(a, (uint)v); WriteWord(a+4, (uint)(v>>32)); }

        // --- Functional Logic ---

        private byte ReadInternal(uint address, bool isInst)
        {
            // 1. Bypass check
            if (!_settings.EnableCacheSimulation || address >= _settings.MmioStartAddress) 
                return _phys.ReadByte(address);

            var cache = isInst ? _l1I : _l1D;
            var metric = isInst ? _metrics.L1I : _metrics.L1D;
            cache.Tick();

            // 2. Try Hit
            if (cache.TryRead(address, out byte val))
            {
                metric.Hits++;
                return val;
            }

            // 3. Miss -> Fetch Block
            metric.Misses++;
            int blockSize = _settings.L1D.LineSizeBytes; // Assuming uniform line size logic for simplicity here
            uint baseAddr = address & ~((uint)blockSize - 1);
            
            // Read block from RAM
            byte[] block = new byte[blockSize];
            for(int i=0; i<blockSize; i++) block[i] = _phys.ReadByte(baseAddr + (uint)i);

            // Fill L1
            cache.Fill(baseAddr, block, out ulong? evAddr, out byte[]? evData);
            
            // Handle Eviction (Writeback)
            if (evAddr.HasValue && evData != null)
            {
                metric.Evictions++;
                for(int i=0; i<evData.Length; i++) 
                    _phys.WriteByte((uint)evAddr.Value + (uint)i, evData[i]);
            }

            return _phys.ReadByte(address); // Return value (now in cache too)
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

            // 1. Try Write Hit
            if (cache.TryWrite(address, val))
            {
                metric.Hits++;
                // If WriteThrough, update RAM immediately
                if (cache.WritePolicy == WritePolicy.WriteThrough)
                    _phys.WriteByte(address, val);
                return;
            }

            // 2. Miss
            metric.Misses++;
            
            if (cache.AllocPolicy == AllocationPolicy.NoWriteAllocate)
            {
                // Bypass cache, write directly to RAM
                _phys.WriteByte(address, val);
            }
            else // Write Allocate
            {
                // Read block from RAM
                int blockSize = _settings.L1D.LineSizeBytes;
                uint baseAddr = address & ~((uint)blockSize - 1);
                byte[] block = new byte[blockSize];
                for(int i=0; i<blockSize; i++) block[i] = _phys.ReadByte(baseAddr + (uint)i);

                // Fill
                cache.Fill(baseAddr, block, out ulong? evAddr, out byte[]? evData);

                // Evict
                if (evAddr.HasValue && evData != null)
                {
                    metric.Evictions++;
                    for(int i=0; i<evData.Length; i++) _phys.WriteByte((uint)evAddr.Value + (uint)i, evData[i]);
                }

                // Retry Write (Hit now)
                cache.TryWrite(address, val);
                if (cache.WritePolicy == WritePolicy.WriteThrough)
                    _phys.WriteByte(address, val);
            }
        }
        
        // This calculates LATENCY only, used by pipeline
        public int GetAccessLatency(uint address, bool isInstruction, bool isWrite)
        {
             // Simplified latency model
             if (!_settings.EnableCacheSimulation) return 0;
             if (address >= _settings.MmioStartAddress) return _settings.DramLatencyCycles;
             
             // In functional mode, we don't strictly separate the latency calc from the action,
             // but the Pipeline calls this to determine stalls.
             // We can assume Hits are fast, Misses slow.
             // Since we track state functionally, we can peek:
             var c = isInstruction ? _l1I : _l1D;
             ulong idx = (address >> 6) & (ulong)((c.Name=="L1I"?_settings.L1I:_settings.L1D).SizeBytes/64 - 1); // rough
             // For proper latency calc in functional mode, we'd need to peek the cache tags without ticking stats
             return c.Latency; 
        }
        
        public int GetAccessLatency(uint address, bool isInstruction) => GetAccessLatency(address, isInstruction, false);
    }
}
