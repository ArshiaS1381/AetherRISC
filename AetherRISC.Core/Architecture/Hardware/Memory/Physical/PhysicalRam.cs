using AetherRISC.Core.Architecture;
using System;
using AetherRISC.Core.Abstractions.Interfaces;

namespace AetherRISC.Core.Architecture.Memory.Physical;

public class PhysicalRam : IMemoryMappedDevice
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

    public byte ReadByte(uint offset) => offset >= Size ? (byte)0 : _memory[offset];
    public void WriteByte(uint offset, byte value) { if (offset < Size) _memory[offset] = value; }

    public uint ReadWord(uint offset)
    {
        if (offset + 3 >= Size) return 0;
        return (uint)(_memory[offset] | (_memory[offset + 1] << 8) | (_memory[offset + 2] << 16) | (_memory[offset + 3] << 24));
    }

    public void WriteWord(uint offset, uint value)
    {
        if (offset + 3 >= Size) return;
        _memory[offset] = (byte)(value & 0xFF);
        _memory[offset + 1] = (byte)((value >> 8) & 0xFF);
        _memory[offset + 2] = (byte)((value >> 16) & 0xFF);
        _memory[offset + 3] = (byte)((value >> 24) & 0xFF);
    }

    public ulong ReadDoubleWord(uint offset)
    {
        uint low = ReadWord(offset);
        uint high = ReadWord(offset + 4);
        return (ulong)low | ((ulong)high << 32);
    }

    public void WriteDoubleWord(uint offset, ulong value)
    {
        WriteWord(offset, (uint)(value & 0xFFFFFFFF));
        WriteWord(offset + 4, (uint)((value >> 32) & 0xFFFFFFFF));
    }
}
