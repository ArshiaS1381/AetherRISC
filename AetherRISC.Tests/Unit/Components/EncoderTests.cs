using Xunit;
using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Hardware.ISA.Encoding;
using AetherRISC.Core.Architecture.Hardware.ISA.Instructions;
using AetherRISC.Core.Architecture.Hardware.ISA.Decoding;

namespace AetherRISC.Tests.Unit.Components;

public class EncoderTests
{
    public EncoderTests() {
        // Ensure static constructor runs to register generators
        new InstructionDecoder(); 
    }

    [Fact]
    public void Encode_Lw_Correctly()
    {
        // LW x1, 100(x0)
        // Op=0000011 (0x03), F3=010 (2), Rd=1, Rs1=0, Imm=100
        var inst = new LwInstruction(1, 0, 100);
        uint raw = InstructionEncoder.Encode(inst);
        
        // Check Opcode
        Assert.Equal(0x03u, raw & 0x7F);
        // Check F3
        Assert.Equal(2u, (raw >> 12) & 0x7);
    }

    [Fact]
    public void Encode_Beq_Correctly()
    {
        // BEQ x1, x2, 16
        // Op=1100011 (0x63), F3=0
        var inst = new BeqInstruction(1, 2, 16);
        uint raw = InstructionEncoder.Encode(inst);
        
        Assert.Equal(0x63u, raw & 0x7F);
    }
}
