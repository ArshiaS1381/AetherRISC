namespace AetherRISC.Core.Abstractions.Interfaces;

public interface IMemoryBus
{
    // 8-bit
    byte ReadByte(uint address);
    void WriteByte(uint address, byte value);

    // 32-bit (Word)
    uint ReadWord(uint address);
    void WriteWord(uint address, uint value);

    // 64-bit (Double Word) - NEW
    ulong ReadDoubleWord(uint address);
    void WriteDoubleWord(uint address, ulong value);
}
