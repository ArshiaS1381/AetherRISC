using AetherRISC.Core.Abstractions.Interfaces;
using System;

namespace AetherRISC.Core.Architecture.Memory.Physical
{
    public class PhysicalRam : IMemoryMappedDevice, IMemoryBus
    {
        private readonly byte[] _memory;
        public uint BaseAddress { get; }
        public uint Size { get; }
        public string Name => "DRAM";

        public PhysicalRam(uint baseAddr, uint sizeBytes)
        {
            BaseAddress = baseAddr;
            Size = sizeBytes;
            _memory = new byte[sizeBytes];
        }

        public byte ReadByte(uint address) => address >= Size ? (byte)0 : _memory[address];
        public void WriteByte(uint address, byte value) { if (address < Size) _memory[address] = value; }

        public ushort ReadHalf(uint address) => (ushort)(ReadByte(address) | (ReadByte(address + 1) << 8));
        public void WriteHalf(uint address, ushort value) { WriteByte(address, (byte)value); WriteByte(address + 1, (byte)(value >> 8)); }

        public uint ReadWord(uint address)
        {
            if (address + 3 >= Size) return 0;
            return (uint)(_memory[address] | (_memory[address + 1] << 8) | (_memory[address + 2] << 16) | (_memory[address + 3] << 24));
        }

        public void WriteWord(uint address, uint value)
        {
            if (address + 3 >= Size) return;
            _memory[address] = (byte)(value & 0xFF);
            _memory[address + 1] = (byte)((value >> 8) & 0xFF);
            _memory[address + 2] = (byte)((value >> 16) & 0xFF);
            _memory[address + 3] = (byte)((value >> 24) & 0xFF);
        }

        public ulong ReadDouble(uint address)
        {
            uint low = ReadWord(address);
            uint high = ReadWord(address + 4);
            return (ulong)low | ((ulong)high << 32);
        }

        public void WriteDouble(uint address, ulong value)
        {
            WriteWord(address, (uint)(value & 0xFFFFFFFF));
            WriteWord(address + 4, (uint)((value >> 32) & 0xFFFFFFFF));
        }
        
        // Legacy support for IMemoryMappedDevice
        public uint ReadWord(uint offset, bool _) => ReadWord(offset);
        public ulong ReadDoubleWord(uint offset) => ReadDouble(offset);
        public void WriteDoubleWord(uint offset, ulong value) => WriteDouble(offset, value);
    }
}
