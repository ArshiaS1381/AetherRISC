/*
 * Project:     AetherRISC
 * File:        IMemoryMappedDevice.cs
 * Version:     1.0.0
 * Description: Interface for any hardware attached to the System Bus.
 */

namespace AetherRISC.Core.Abstractions.Interfaces;

public interface IMemoryMappedDevice
{
    uint BaseAddress { get; }
    uint Size { get; }
    string Name { get; }

    byte ReadByte(uint offset);
    void WriteByte(uint offset, byte value);
    
    // Devices must implement these to handle endianness internally
    uint ReadWord(uint offset);
    void WriteWord(uint offset, uint value);
    ulong ReadDoubleWord(uint offset);
    void WriteDoubleWord(uint offset, ulong value);
}
