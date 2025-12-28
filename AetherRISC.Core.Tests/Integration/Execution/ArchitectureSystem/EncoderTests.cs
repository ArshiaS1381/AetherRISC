using Xunit;
using AetherRISC.Core.Architecture.Hardware.ISA.Instructions;
using AetherRISC.Core.Architecture.Hardware.ISA.Encoding;
using AetherRISC.Core.Architecture.Hardware.ISA.Decoding; // For Decoder instantiation

namespace AetherRISC.Core.Tests.Architecture.System;

public class EncoderTests
{
    public EncoderTests()
    {
        // Force registration of families by creating a decoder
        new InstructionDecoder();
    }

    [Fact]
    public void LW_Should_Encode_Correctly()
    {
        // LW x1, 100(x0)
        // Op=0x03, F3=2, Rd=1, Rs1=0, Imm=100
        var inst = new LwInstruction(1, 0, 100);
        
        uint encoded = InstructionEncoder.Encode(inst);
        
        // Verify Opcode is 0x03 (Load), not 0x13 (Imm/NOP)
        uint opcode = encoded & 0x7F;
        Assert.Equal(0x03u, opcode);
        
        // Verify F3 is 2 (LW), not 3 (LD)
        uint f3 = (encoded >> 12) & 0x7;
        Assert.Equal(2u, f3);
    }

    [Fact]
    public void ADDI_Should_Encode_Correctly()
    {
        var inst = new AddiInstruction(2, 1, 5);
        uint encoded = InstructionEncoder.Encode(inst);
        
        uint opcode = encoded & 0x7F;
        Assert.Equal(0x13u, opcode);
    }
}



