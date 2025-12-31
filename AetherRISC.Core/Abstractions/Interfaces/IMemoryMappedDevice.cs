namespace AetherRISC.Core.Abstractions.Interfaces;

public interface IMemoryMappedDevice
{
    uint BaseAddress { get; }
    uint Size { get; }
    string Name { get; }

    byte ReadByte(uint offset);
    void WriteByte(uint offset, byte value);
    
    uint ReadWord(uint offset);
    void WriteWord(uint offset, uint value);
    ulong ReadDoubleWord(uint offset);
    void WriteDoubleWord(uint offset, ulong value);
}
