using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using AetherRISC.Core.Abstractions.Interfaces;

namespace AetherRISC.Core.Architecture.Hardware.Memory
{
    public unsafe class SystemBus : IMemoryBus
    {
        private const int PageSize = 4096;
        private const int PageShift = 12;
        private const uint PageMask = 0xFFF;

        private readonly Dictionary<uint, byte[]> _pages = new();
        private readonly uint _size;

        private uint _key0 = uint.MaxValue;
        private byte[]? _val0 = null;
        
        private uint _key1 = uint.MaxValue;
        private byte[]? _val1 = null;

        public SystemBus(uint size)
        {
            _size = size;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private byte[]? GetPage(uint address, bool create)
        {
            uint pfn = address >> PageShift;

            if (pfn == _key0) return _val0;

            if (pfn == _key1) 
            {
                var tmpK = _key0; var tmpV = _val0;
                _key0 = _key1;    _val0 = _val1;
                _key1 = tmpK;     _val1 = tmpV;
                return _val0;
            }

            if (_pages.TryGetValue(pfn, out var page)) 
            {
                _key1 = _key0; _val1 = _val0;
                _key0 = pfn;   _val0 = page;
                return page;
            }
            
            if (!create) return null;

            page = new byte[PageSize];
            _pages[pfn] = page;
            
            _key1 = _key0; _val1 = _val0;
            _key0 = pfn;   _val0 = page;
            
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
            if ((address & PageMask) <= PageSize - 2)
            {
                var page = GetPage(address, false);
                if (page == null) return 0;
                fixed (byte* ptr = &page![address & PageMask]) { return *(ushort*)ptr; }
            }
            return (ushort)(ReadByte(address) | (ReadByte(address + 1) << 8));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteHalf(uint address, ushort value)
        {
            if ((address & PageMask) <= PageSize - 2)
            {
                var page = GetPage(address, true);
                fixed (byte* ptr = &page![address & PageMask]) { *(ushort*)ptr = value; }
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
                fixed (byte* ptr = &page![address & PageMask]) { return *(uint*)ptr; }
            }
            return (uint)(ReadByte(address) | (ReadByte(address + 1) << 8) | (ReadByte(address + 2) << 16) | (ReadByte(address + 3) << 24));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteWord(uint address, uint value)
        {
            if ((address & PageMask) <= PageSize - 4)
            {
                var page = GetPage(address, true);
                fixed (byte* ptr = &page![address & PageMask]) { *(uint*)ptr = value; }
            }
            else
            {
                WriteByte(address, (byte)(value & 0xFF));
                WriteByte(address + 1, (byte)((value >> 8) & 0xFF));
                WriteByte(address + 2, (byte)((value >> 16) & 0xFF));
                WriteByte(address + 3, (byte)((value >> 24) & 0xFF));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ulong ReadDouble(uint address)
        {
            if ((address & PageMask) <= PageSize - 8)
            {
                var page = GetPage(address, false);
                if (page == null) return 0;
                fixed (byte* ptr = &page![address & PageMask]) { return *(ulong*)ptr; }
            }
            uint w0 = ReadWord(address);
            uint w1 = ReadWord(address + 4);
            return (ulong)w0 | ((ulong)w1 << 32);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteDouble(uint address, ulong value)
        {
            if ((address & PageMask) <= PageSize - 8)
            {
                var page = GetPage(address, true);
                fixed (byte* ptr = &page![address & PageMask]) { *(ulong*)ptr = value; }
            }
            else
            {
                WriteWord(address, (uint)(value & 0xFFFFFFFF));
                WriteWord(address + 4, (uint)((value >> 32) & 0xFFFFFFFF));
            }
        }
    }
}
