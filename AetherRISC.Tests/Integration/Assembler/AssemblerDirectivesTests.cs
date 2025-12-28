using Xunit;
using AetherRISC.Tests.Infrastructure;
using AetherRISC.Core.Helpers;

namespace AetherRISC.Tests.Integration.Assembler;

public class AssemblerDirectivesTests : CpuTestFixture
{
    [Fact]
    public void Data_Directives_Write_To_Memory()
    {
        Init64();

        string code = @"
            .data
            byteVal: .byte 0xAA
            halfVal: .half 0xBBAA
            wordVal: .word 0xDEADBEEF

            .text
            main:
                nop
        ";

        var asm = new SourceAssembler(code);
        asm.Assemble(Machine);

        uint baseAddr = 0x10010000;

        // .byte 0xAA at base
        Assert.Equal(0xAA, Memory.ReadByte(baseAddr + 0));

        // padding due to .half alignment to 2 bytes
        Assert.Equal(0x00, Memory.ReadByte(baseAddr + 1));

        // .half 0xBBAA at base+2 (little-endian: AA then BB)
        Assert.Equal(0xAA, Memory.ReadByte(baseAddr + 2));
        Assert.Equal(0xBB, Memory.ReadByte(baseAddr + 3));

        // .word aligned to 4: base+4 (little-endian)
        Assert.Equal(0xEF, Memory.ReadByte(baseAddr + 4));
        Assert.Equal(0xBE, Memory.ReadByte(baseAddr + 5));
        Assert.Equal(0xAD, Memory.ReadByte(baseAddr + 6));
        Assert.Equal(0xDE, Memory.ReadByte(baseAddr + 7));
    }

    [Fact]
    public void Labels_Resolve_Correctly_In_Text()
    {
        Init64();
        string code = @"
            .text
            start:
                addi x1, x0, 10
                j target
                addi x1, x0, 0
            target:
                addi x1, x1, 5
        ";

        var asm = new SourceAssembler(code) { TextBase = (uint)Machine.Config.ResetVector };
        asm.Assemble(Machine);

        Runner.Run(4);

        AssertReg(1, 15ul);
    }
}
