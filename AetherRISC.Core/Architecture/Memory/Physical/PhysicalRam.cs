namespace AetherRISC.Core.Architecture.Memory.Physical;

public class PhysicalRam
{
    private readonly byte[] _memory;
    public uint Size { get; }

    public PhysicalRam(uint sizeBytes)
    {
        Size = sizeBytes;
        _memory = new byte[sizeBytes];
    }

    // Basic Byte Access
    public byte ReadByte(uint offset)
    {
        if (offset >= Size) return 0; // Faults handled by bus later
        return _memory[offset];
    }

    public void WriteByte(uint offset, byte value)
    {
        if (offset < Size) _memory[offset] = value;
    }

    // Word Access (Little Endian for RISC-V)
    public uint ReadWord(uint offset)
    {
        if (offset + 3 >= Size) return 0;
        return (uint)(_memory[offset] |
                     (_memory[offset + 1] << 8) |
                     (_memory[offset + 2] << 16) |
                     (_memory[offset + 3] << 24));
    }

    public void WriteWord(uint offset, uint value)
    {
        if (offset + 3 >= Size) return;
        _memory[offset]     = (byte)(value & 0xFF);
        _memory[offset + 1] = (byte)((value >> 8) & 0xFF);
        _memory[offset + 2] = (byte)((value >> 16) & 0xFF);
        _memory[offset + 3] = (byte)((value >> 24) & 0xFF);
    }
}
