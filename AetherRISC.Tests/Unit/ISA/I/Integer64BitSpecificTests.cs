using Xunit;
using AetherRISC.Tests.Infrastructure;
using AetherRISC.Core.Helpers;

namespace AetherRISC.Tests.Unit.ISA.I;

public class Integer64BitSpecificTests : CpuTestFixture
{
    [Fact]
    public void Addiw_SignExtends_32Bit_Result()
    {
        Init64();

        string code = @"
            li x1, 0x7FFFFFFF
            addiw x2, x1, 1
        ";

        var asm = new SourceAssembler(code) { TextBase = 0 };
        asm.Assemble(Machine);

        Runner.Run(10);

        AssertReg(2, 0xFFFFFFFF80000000ul);
    }
}
