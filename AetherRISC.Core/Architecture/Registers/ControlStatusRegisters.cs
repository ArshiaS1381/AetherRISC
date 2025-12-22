using System.Collections.Generic;

namespace AetherRISC.Core.Architecture.Registers;

public class ControlStatusRegisters
{
    // CSRs are 12-bit addressed (0x000 to 0xFFF).
    // Using a dictionary is more efficient than a 4096-element array since most are unused.
    private readonly Dictionary<uint, ulong> _csrs = new Dictionary<uint, ulong>();

    // Common CSR Addresses
    public const uint MSTATUS = 0x300;
    public const uint MISA    = 0x301;
    public const uint MEPC    = 0x341;
    public const uint MCAUSE  = 0x342;

    public ulong Read(uint address)
    {
        // TODO: Implement read side-effects (e.g. reading time/cycle)
        return _csrs.ContainsKey(address) ? _csrs[address] : 0;
    }

    public void Write(uint address, ulong value)
    {
        // TODO: Implement write masks (some bits are read-only)
        _csrs[address] = value;
    }

    public void Reset()
    {
        _csrs.Clear();
    }
}
