using AetherRISC.Core.Architecture.Hardware.ISA.Decoding;
using AetherRISC.Core.Architecture.Hardware.ISA.Encoding;
using AetherRISC.Core.Helpers;
using Xunit;

namespace AetherRISC.Core.Tests.Architecture.Stress;

public class ZbbEncodingDecodingDiagnostics
{
    [Fact]
    public void ZextH_Encodes_And_Decodes_As_ZextH()
    {
        // zext.h t2, t0  => rd=7 rs1=5
        uint word = InstructionEncoder.Encode(Inst.ZextH(7, 5));

        // Check encoding shape: opcode 0x13, funct3 1, funct7 0x30, sel 6
        Assert.Equal(0x13u, word & 0x7Fu);
        Assert.Equal(1u, (word >> 12) & 0x7u);
        Assert.Equal(0x30u, (word >> 25) & 0x7Fu);
        Assert.Equal(6u, (word >> 20) & 0x1Fu);

        var dec = new InstructionDecoder();
        var inst = dec.Decode(word);

        Assert.Equal("ZEXT.H", inst.Mnemonic);
        Assert.Equal(7, inst.Rd);
        Assert.Equal(5, inst.Rs1);
    }
}
