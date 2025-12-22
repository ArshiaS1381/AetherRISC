using System;
using AetherRISC.Core.Abstractions.Interfaces;

namespace AetherRISC.Core.Architecture.Memory;

public class SystemBus : IMemoryBus
{
    private readonly byte[] _memory;

    public SystemBus(uint size)
    {
        _memory = new byte[size];
    }

    public byte ReadByte(uint address)
    {
        if (address >= _memory.Length) return 0;
        return _memory[address];
    }

    public void WriteByte(uint address, byte value)
    {
        if (address < _memory.Length)
        {
            _memory[address] = value;
        }
    }

    // --- 32-bit Word ---
    public uint ReadWord(uint address)
    {
        // Little Endian: [0] | [1]<<8 | [2]<<16 | [3]<<24
        return (uint)(ReadByte(address) | 
                      (ReadByte(address + 1) << 8) |
                      (ReadByte(address + 2) << 16) |
                      (ReadByte(address + 3) << 24));
    }

    public void WriteWord(uint address, uint value)
    {
        WriteByte(address, (byte)(value & 0xFF));
        WriteByte(address + 1, (byte)((value >> 8) & 0xFF));
        WriteByte(address + 2, (byte)((value >> 16) & 0xFF));
        WriteByte(address + 3, (byte)((value >> 24) & 0xFF));
    }

    // --- 64-bit Double Word (NEW) ---
    public ulong ReadDoubleWord(uint address)
    {
        // Combine two 32-bit reads
        uint low = ReadWord(address);
        uint high = ReadWord(address + 4);
        return (ulong)low | ((ulong)high << 32);
    }

    public void WriteDoubleWord(uint address, ulong value)
    {
        // Split into two 32-bit writes
        WriteWord(address, (uint)(value & 0xFFFFFFFF));
        WriteWord(address + 4, (uint)((value >> 32) & 0xFFFFFFFF));
    }
}
