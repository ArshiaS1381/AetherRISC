using System;
using AetherRISC.Core.Abstractions.Interfaces;
namespace AetherRISC.Core.Architecture.Memory;
public class MemoryController : IMemoryBus {
    private readonly byte[] _ram;
    public MemoryController(uint size) { _ram = new byte[size]; }
    public byte ReadByte(uint addr) => addr < _ram.Length ? _ram[addr] : (byte)0;
    public void WriteByte(uint addr, byte v) { if (addr < _ram.Length) _ram[addr] = v; }
    public uint ReadWord(uint addr) => (uint)(ReadByte(addr) | (ReadByte(addr+1)<<8) | (ReadByte(addr+2)<<16) | (ReadByte(addr+3)<<24));
    public void WriteWord(uint addr, uint v) { WriteByte(addr, (byte)v); WriteByte(addr+1, (byte)(v>>8)); WriteByte(addr+2, (byte)(v>>16)); WriteByte(addr+3, (byte)(v>>24)); }
    public ulong ReadDoubleWord(uint addr) => (ulong)ReadWord(addr) | ((ulong)ReadWord(addr+4) << 32);
    public void WriteDoubleWord(uint addr, ulong v) { WriteWord(addr, (uint)v); WriteWord(addr+4, (uint)(v>>32)); }
}
