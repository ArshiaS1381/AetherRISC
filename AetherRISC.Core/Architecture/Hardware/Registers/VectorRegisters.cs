using AetherRISC.Core.Architecture;
using System;

namespace AetherRISC.Core.Architecture.Hardware.Registers;

public class VectorRegisters
{
    private readonly byte[][] _registers;
    public int VLenBytes { get; }
    public ulong Vstart { get; set; }
    public ulong Vxsat { get; set; }
    public ulong Vxrm { get; set; }
    public ulong Vcsr { get; set; }
    public ulong Vtype { get; set; }
    public ulong Vlenb { get; private set; }

    public VectorRegisters(int vlenBits = 128)
    {
        VLenBytes = vlenBits / 8;
        Vlenb = (ulong)VLenBytes;
        _registers = new byte[32][];
        for (int i = 0; i < 32; i++) _registers[i] = new byte[VLenBytes];
    }

    public byte[] Read(int index) => (index < 0 || index >= 32) ? Array.Empty<byte>() : _registers[index];
    public void Write(int index, byte[] data)
    {
        if (index < 0 || index >= 32 || data.Length != VLenBytes) return;
        Array.Copy(data, _registers[index], VLenBytes);
    }
    public void Reset() { for (int i = 0; i < 32; i++) Array.Clear(_registers[i], 0, VLenBytes); Vstart = 0; }
}
