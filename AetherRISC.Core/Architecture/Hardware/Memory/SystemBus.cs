using AetherRISC.Core.Abstractions.Interfaces;
using System.Collections.Generic;

namespace AetherRISC.Core.Architecture.Hardware.Memory
{
    public class SystemBus : IMemoryBus
    {
        // Simple byte-addressable memory map for emulation
        // Using a Dictionary for sparse storage to avoid allocating 4GB arrays
        private readonly Dictionary<uint, byte> _memory = new Dictionary<uint, byte>();
        private readonly uint _size;

        public SystemBus(uint size)
        {
            _size = size;
        }

        // --- 8-bit ---
        public byte ReadByte(uint address) 
            => _memory.TryGetValue(address, out var val) ? val : (byte)0;

        public void WriteByte(uint address, byte value) 
            => _memory[address] = value;

        // --- 16-bit (Little Endian) ---
        public ushort ReadHalf(uint address)
        {
            byte b0 = ReadByte(address);
            byte b1 = ReadByte(address + 1);
            return (ushort)(b0 | (b1 << 8));
        }

        public void WriteHalf(uint address, ushort value)
        {
            WriteByte(address, (byte)(value & 0xFF));
            WriteByte(address + 1, (byte)((value >> 8) & 0xFF));
        }

        // --- 32-bit (Little Endian) ---
        public uint ReadWord(uint address)
        {
            ushort h0 = ReadHalf(address);
            ushort h1 = ReadHalf(address + 2);
            return (uint)(h0 | (h1 << 16));
        }

        public void WriteWord(uint address, uint value)
        {
            WriteHalf(address, (ushort)(value & 0xFFFF));
            WriteHalf(address + 2, (ushort)((value >> 16) & 0xFFFF));
        }

        // --- 64-bit (Little Endian) ---
        public ulong ReadDouble(uint address)
        {
            uint w0 = ReadWord(address);
            uint w1 = ReadWord(address + 4);
            return (ulong)w0 | ((ulong)w1 << 32);
        }

        public void WriteDouble(uint address, ulong value)
        {
            WriteWord(address, (uint)(value & 0xFFFFFFFF));
            WriteWord(address + 4, (uint)((value >> 32) & 0xFFFFFFFF));
        }
    }
}
