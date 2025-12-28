using System.Collections.Generic;

namespace AetherRISC.Core.Architecture.Hardware.Registers;

public class CsrFile
{
    private readonly Dictionary<uint, ulong> _csrs = new();

    public ulong Read(uint address) 
    {
        return _csrs.ContainsKey(address) ? _csrs[address] : 0;
    }

    public void Write(uint address, ulong value)
    {
        _csrs[address] = value;
    }
}
