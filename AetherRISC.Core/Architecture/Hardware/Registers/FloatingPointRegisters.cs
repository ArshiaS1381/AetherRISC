using System;

namespace AetherRISC.Core.Architecture.Hardware.Registers;

public class FloatingPointRegisters
{
    // Store raw 64-bit payload per register (RISC-V FPRs are XLEN-wide in RV64).
    private readonly ulong[] _bits = new ulong[32];

    // --- Raw access (useful for debugging) ---
    public ulong ReadRaw(int index) => index is < 0 or >= 32 ? 0ul : _bits[index];
    public void WriteRaw(int index, ulong value)
    {
        if (index is < 0 or >= 32) return;
        _bits[index] = value;
    }

    // --- Double (64-bit) ---
    public double ReadDouble(int index)
    {
        ulong raw = ReadRaw(index);
        return BitConverter.UInt64BitsToDouble(raw);
    }

    public void WriteDouble(int index, double value)
    {
        ulong raw = BitConverter.DoubleToUInt64Bits(value);
        WriteRaw(index, raw);
    }

    // --- Single (32-bit) with NaN-boxing on RV64 ---
    // RISC-V requires upper 32 bits to be all 1s when a 32-bit float is stored in an FPR.
    public float ReadSingle(int index)
    {
        uint lo = (uint)(ReadRaw(index) & 0xFFFFFFFFu);
        return BitConverter.UInt32BitsToSingle(lo);
    }

    public void WriteSingle(int index, float value)
    {
        uint lo = BitConverter.SingleToUInt32Bits(value);
        ulong raw = 0xFFFFFFFF00000000ul | lo;
        WriteRaw(index, raw);
    }

    // Legacy convenience API (maps to double semantics)
    public double Read(int index) => ReadDouble(index);
    public void Write(int index, double value) => WriteDouble(index, value);
}
