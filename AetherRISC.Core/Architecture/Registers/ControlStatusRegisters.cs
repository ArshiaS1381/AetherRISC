using AetherRISC.Core.Architecture;
using System.Collections.Generic;

namespace AetherRISC.Core.Architecture.Registers;

public class ControlStatusRegisters
{
    private readonly Dictionary<uint, ulong> _csrs = new Dictionary<uint, ulong>();
    public const uint MSTATUS = 0x300;
    public const uint MISA    = 0x301;
    public const uint MEPC    = 0x341;
    public const uint MCAUSE  = 0x342;

    public ulong Read(uint address) => _csrs.ContainsKey(address) ? _csrs[address] : 0;
    public void Write(uint address, ulong value) => _csrs[address] = value;
    public void Reset() => _csrs.Clear();
}
