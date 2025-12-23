using Xunit;
using AetherRISC.Core.Architecture.ISA.Encoding;
using AetherRISC.Core.Architecture.ISA.Decoding;
using AetherRISC.Core.Architecture.ISA.Base;
using AetherRISC.Core.Helpers;

namespace AetherRISC.Core.Tests.Architecture.System;

public class CodecTests
{
    private InstructionDecoder _decoder = new InstructionDecoder();

    [Fact]
    public void RoundTrip_Arithmetic_R_Type()
    {
        // ADD x10, x11, x12
        var original = Inst.Add(10, 11, 12);
        uint encoded = InstructionEncoder.Encode(original);
        var decoded = _decoder.Decode(encoded);

        Assert.Equal("ADD", decoded.Mnemonic);
        Assert.Equal(10, decoded.Rd);
        Assert.Equal(11, decoded.Rs1);
        Assert.Equal(12, decoded.Rs2);
    }

    [Fact]
    public void RoundTrip_Immediate_I_Type()
    {
        // ADDI x5, x6, -123
        var original = Inst.Addi(5, 6, -123);
        uint encoded = InstructionEncoder.Encode(original);
        var decoded = _decoder.Decode(encoded);

        Assert.Equal("ADDI", decoded.Mnemonic);
        Assert.Equal(5, decoded.Rd);
        Assert.Equal(6, decoded.Rs1);
        // Verify sign extension is handled correctly in comparison
        Assert.Equal(-123, decoded.Imm); 
    }

    [Fact]
    public void RoundTrip_Store_S_Type()
    {
        // SW x2, 40(x3) -> Store x2 into Address(x3 + 40)
        // Src=x2, Base=x3
        var original = Inst.Sw(3, 2, 40); 
        uint encoded = InstructionEncoder.Encode(original);
        var decoded = _decoder.Decode(encoded);

        Assert.Equal("SW", decoded.Mnemonic);
        Assert.Equal(3, decoded.Rs1); // Base
        Assert.Equal(2, decoded.Rs2); // Src
        Assert.Equal(40, decoded.Imm);
    }

    [Fact]
    public void RoundTrip_Branch_B_Type()
    {
        // BNE x1, x2, -16
        var original = Inst.Bne(1, 2, -16);
        uint encoded = InstructionEncoder.Encode(original);
        var decoded = _decoder.Decode(encoded);

        Assert.Equal("BNE", decoded.Mnemonic);
        Assert.Equal(1, decoded.Rs1);
        Assert.Equal(2, decoded.Rs2);
        Assert.Equal(-16, decoded.Imm);
    }
}
