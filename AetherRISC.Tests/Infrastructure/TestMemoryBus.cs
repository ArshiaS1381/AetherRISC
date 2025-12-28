using System;
using System.Collections.Generic;
using AetherRISC.Core.Abstractions.Interfaces;

namespace AetherRISC.Tests.Infrastructure;

public class TestMemoryBus : IMemoryBus
{
    private readonly Dictionary<uint, byte> _mem = new();

    // Compatibility constructor (size is ignored for sparse memory)
    public TestMemoryBus(int _ignoredSizeInBytes = 0)
    {
    }

    // --- Helpers ---
    private byte ReadByteInternal(uint addr)
        => _mem.TryGetValue(addr, out var b) ? b : (byte)0;

    private void WriteByteInternal(uint addr, byte val)
        => _mem[addr] = val;

    // --- 8-bit ---
    public byte ReadByte(uint address)
        => ReadByteInternal(address);

    public void WriteByte(uint address, byte value)
        => WriteByteInternal(address, value);

    // --- 16-bit ---
    public ushort ReadHalf(uint address)
    {
        return (ushort)(
            ReadByteInternal(address)
            | (ReadByteInternal(address + 1) << 8)
        );
    }

    public void WriteHalf(uint address, ushort value)
    {
        WriteByteInternal(address,     (byte)(value & 0xFF));
        WriteByteInternal(address + 1, (byte)((value >> 8) & 0xFF));
    }

    // --- 32-bit ---
    public uint ReadWord(uint address)
    {
        return
            (uint)ReadByteInternal(address)
            | ((uint)ReadByteInternal(address + 1) << 8)
            | ((uint)ReadByteInternal(address + 2) << 16)
            | ((uint)ReadByteInternal(address + 3) << 24);
    }

    public void WriteWord(uint address, uint value)
    {
        WriteByteInternal(address,     (byte)(value & 0xFF));
        WriteByteInternal(address + 1, (byte)((value >> 8) & 0xFF));
        WriteByteInternal(address + 2, (byte)((value >> 16) & 0xFF));
        WriteByteInternal(address + 3, (byte)((value >> 24) & 0xFF));
    }

    // --- 64-bit ---
    public ulong ReadDouble(uint address)
    {
        return
            (ulong)ReadWord(address)
            | ((ulong)ReadWord(address + 4) << 32);
    }

    public void WriteDouble(uint address, ulong value)
    {
        WriteWord(address,     (uint)(value & 0xFFFFFFFF));
        WriteWord(address + 4, (uint)(value >> 32));
    }
}
