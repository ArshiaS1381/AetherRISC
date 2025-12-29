using Xunit;
using AetherRISC.Core.Architecture.Hardware.ISA.Decoding;

namespace AetherRISC.Tests.Unit.ISA.C;

public class DecompressionTests
{
    [Fact]
    public void Decompress_C_ADDI_To_ADDI()
    {
        // Simulating C.NOP (ADDI x0, x0, 0) -> 0x0001
        ushort c_nop = 0x0001;
        uint raw = InstructionDecompressor.Decompress(c_nop);
        
        // Expected: ADDI x0, x0, 0 -> 0x00000013
        Assert.Equal(0x00000013u, raw);
    }

    [Fact]
    public void IsCompressed_Detects_16Bit_Alignment()
    {
        // Compressed instructions end in 00, 01, or 10.
        // Standard (32-bit) end in 11.
        
        Assert.True(InstructionDecompressor.IsCompressed(0x0000)); // Ends 00
        Assert.True(InstructionDecompressor.IsCompressed(0x0001)); // Ends 01
        Assert.True(InstructionDecompressor.IsCompressed(0x0002)); // Ends 10
        Assert.False(InstructionDecompressor.IsCompressed(0x0003)); // Ends 11
    }
}
