using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using AetherRISC.Core.Abstractions.Interfaces;

namespace AetherRISC.Core.Architecture.Hardware.Memory
{
    public class SystemBus : IMemoryBus
    {
        private const int PageSize = 4096;
        private const int PageShift = 12;
        private const uint PageMask = 0xFFF;
        private readonly Dictionary<uint, byte[]> _pages = new();
        private readonly uint _size;

        public SystemBus(uint size) => _size = size;

        private byte[]? GetPage(uint address, bool create)
        {
            uint pfn = address >> PageShift;
            if (_pages.TryGetValue(pfn, out var page)) return page;
            if (!create) return null;
            page = new byte[PageSize];
            _pages[pfn] = page;
            return page;
        }

        public byte ReadByte(uint address) 
        {
            var page = GetPage(address, false);
            return page == null ? (byte)0 : page[address & PageMask];
        }

        public void WriteByte(uint address, byte value) 
        {
            var page = GetPage(address, true);
            page![address & PageMask] = value;
        }

        public ushort ReadHalf(uint a) => (ushort)(ReadByte(a) | (ReadByte(a + 1) << 8));
        public void WriteHalf(uint a, ushort v) { WriteByte(a, (byte)v); WriteByte(a+1, (byte)(v>>8)); }
        public uint ReadWord(uint a) => (uint)(ReadHalf(a) | (ReadHalf(a + 2) << 16));
        public void WriteWord(uint a, uint v) { WriteHalf(a, (ushort)v); WriteHalf(a+2, (ushort)(v>>16)); }
        public ulong ReadDouble(uint a) => (ulong)ReadWord(a) | ((ulong)ReadWord(a + 4) << 32);
        public void WriteDouble(uint a, ulong v) { WriteWord(a, (uint)v); WriteWord(a+4, (uint)(v>>32)); }
    }
}
