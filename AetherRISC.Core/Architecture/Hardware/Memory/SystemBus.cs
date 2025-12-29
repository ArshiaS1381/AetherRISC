using AetherRISC.Core.Abstractions.Interfaces;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace AetherRISC.Core.Architecture.Hardware.Memory
{
    public class SystemBus : IMemoryBus
    {
        // 4KB Pages
        private const int PageSize = 4096;
        private const int PageShift = 12;
        private const uint PageMask = 0xFFF;

        // Sparse storage
        private readonly Dictionary<uint, byte[]> _pages = new();
        private readonly uint _size;

        // --- Optimization: Last Page Cache ---
        // Accessing the same page sequentially is the most common operation (fetching).
        // Caching the last accessed array skips the Dictionary lookup.
        private uint _lastPageKey = uint.MaxValue;
        private byte[]? _lastPageCache = null;

        public SystemBus(uint size)
        {
            _size = size;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private byte[]? GetPage(uint address, bool create)
        {
            uint pfn = address >> PageShift;

            // 1. Fast Path: Hit the cache
            if (pfn == _lastPageKey && _lastPageCache != null) 
                return _lastPageCache;

            // 2. Slow Path: Dictionary Lookup
            if (_pages.TryGetValue(pfn, out var page)) 
            {
                // Update Cache
                _lastPageKey = pfn;
                _lastPageCache = page;
                return page;
            }
            
            if (!create) return null;

            // 3. Allocation
            page = new byte[PageSize];
            _pages[pfn] = page;
            
            // Update Cache
            _lastPageKey = pfn;
            _lastPageCache = page;
            
            return page;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte ReadByte(uint address) 
        {
            var page = GetPage(address, false);
            return page == null ? (byte)0 : page[address & PageMask];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteByte(uint address, byte value) 
        {
            var page = GetPage(address, true);
            page![address & PageMask] = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ushort ReadHalf(uint address)
        {
            // Fast Path: Aligned within a single page
            if ((address & PageMask) <= PageSize - 2)
            {
                var page = GetPage(address, false);
                if (page == null) return 0;
                uint offset = address & PageMask;
                return (ushort)(page[offset] | (page[offset + 1] << 8));
            }
            // Slow Path: Crossing Page Boundary
            return (ushort)(ReadByte(address) | (ReadByte(address + 1) << 8));
        }

        public void WriteHalf(uint address, ushort value)
        {
            if ((address & PageMask) <= PageSize - 2)
            {
                var page = GetPage(address, true);
                uint offset = address & PageMask;
                page![offset] = (byte)(value & 0xFF);
                page[offset + 1] = (byte)((value >> 8) & 0xFF);
            }
            else
            {
                WriteByte(address, (byte)(value & 0xFF));
                WriteByte(address + 1, (byte)((value >> 8) & 0xFF));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint ReadWord(uint address)
        {
            if ((address & PageMask) <= PageSize - 4)
            {
                var page = GetPage(address, false);
                if (page == null) return 0;
                uint offset = address & PageMask;
                return (uint)(page[offset] | (page[offset + 1] << 8) | (page[offset + 2] << 16) | (page[offset + 3] << 24));
            }
            return (uint)(ReadByte(address) | (ReadByte(address + 1) << 8) | (ReadByte(address + 2) << 16) | (ReadByte(address + 3) << 24));
        }

        public void WriteWord(uint address, uint value)
        {
            if ((address & PageMask) <= PageSize - 4)
            {
                var page = GetPage(address, true);
                uint offset = address & PageMask;
                page![offset] = (byte)(value & 0xFF);
                page[offset + 1] = (byte)((value >> 8) & 0xFF);
                page[offset + 2] = (byte)((value >> 16) & 0xFF);
                page[offset + 3] = (byte)((value >> 24) & 0xFF);
            }
            else
            {
                WriteByte(address, (byte)(value & 0xFF));
                WriteByte(address + 1, (byte)((value >> 8) & 0xFF));
                WriteByte(address + 2, (byte)((value >> 16) & 0xFF));
                WriteByte(address + 3, (byte)((value >> 24) & 0xFF));
            }
        }

        public ulong ReadDouble(uint address)
        {
            if ((address & PageMask) <= PageSize - 8)
            {
                var page = GetPage(address, false);
                if (page == null) return 0;
                uint offset = address & PageMask;
                uint lo = (uint)(page[offset] | (page[offset + 1] << 8) | (page[offset + 2] << 16) | (page[offset + 3] << 24));
                uint hi = (uint)(page[offset + 4] | (page[offset + 5] << 8) | (page[offset + 6] << 16) | (page[offset + 7] << 24));
                return (ulong)lo | ((ulong)hi << 32);
            }
            
            uint w0 = ReadWord(address);
            uint w1 = ReadWord(address + 4);
            return (ulong)w0 | ((ulong)w1 << 32);
        }

        public void WriteDouble(uint address, ulong value)
        {
            if ((address & PageMask) <= PageSize - 8)
            {
                var page = GetPage(address, true);
                uint offset = address & PageMask;
                page![offset] = (byte)value;
                page[offset + 1] = (byte)(value >> 8);
                page[offset + 2] = (byte)(value >> 16);
                page[offset + 3] = (byte)(value >> 24);
                page[offset + 4] = (byte)(value >> 32);
                page[offset + 5] = (byte)(value >> 40);
                page[offset + 6] = (byte)(value >> 48);
                page[offset + 7] = (byte)(value >> 56);
            }
            else
            {
                WriteWord(address, (uint)(value & 0xFFFFFFFF));
                WriteWord(address + 4, (uint)((value >> 32) & 0xFFFFFFFF));
            }
        }
    }
}
