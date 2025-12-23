namespace AetherRISC.Core.Abstractions.Interfaces
{
    public interface IMemoryBus
    {
        // 8-bit
        byte ReadByte(uint address);
        void WriteByte(uint address, byte value);
        
        // 16-bit (Half)
        ushort ReadHalf(uint address);
        void WriteHalf(uint address, ushort value);

        // 32-bit (Word)
        uint ReadWord(uint address);
        void WriteWord(uint address, uint value);

        // 64-bit (Double) - NEW
        ulong ReadDouble(uint address);
        void WriteDouble(uint address, ulong value);
    }
}
