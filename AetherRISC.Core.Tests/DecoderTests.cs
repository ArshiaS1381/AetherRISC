using Xunit;
using AetherRISC.Core.Hardware.ISA.Decoding;
using AetherRISC.Core.Hardware.ISA.Base;
using AetherRISC.Core.Architecture;

namespace AetherRISC.Core.Tests;

public class DecoderTests
{
    [Fact]
    public void Should_Decode_Raw_Hex_To_ADDI()
    {
        uint rawHex = 0x00508113;
        var decoder = new InstructionDecoder();
        var instruction = decoder.Decode(rawHex);
        Assert.IsType<AddiInstruction>(instruction);
        // FIX: Use single double-quotes for the string literal
        Assert.Equal("ADDI", instruction.Mnemonic);
    }
}
