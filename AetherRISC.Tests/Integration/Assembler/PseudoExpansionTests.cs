using Xunit;
using AetherRISC.Tests.Infrastructure;
using AetherRISC.Core.Helpers; // SourceAssembler

namespace AetherRISC.Tests.Integration.Assembler;

public class PseudoExpansionTests : CpuTestFixture
{
    [Fact]
    public void Li_Expansion_Large_Negative_Simulated()
    {
        Init64();
        string code = "li x1, -1";

        var asm = new SourceAssembler(code) { TextBase = 0 };
        asm.Assemble(Machine);

        Runner.Run(1);
        AssertReg(1, 0xFFFFFFFFFFFFFFFFul);
    }

    [Fact]
    public void Li_Expansion_Bit31_SignExtension_Check()
    {
        Init64();
        string code = @"
            li t0, 0x7FFFFFFF
            li t1, 0x80000000
        ";

        var asm = new SourceAssembler(code) { TextBase = 0 };
        asm.Assemble(Machine);

        Runner.Run(4);

        AssertReg(5, 0x7FFFFFFFul); // t0

        // RV64 LUI sign-extends bit 31, so 0x80000000 becomes 0xFFFFFFFF80000000
        AssertReg(6, 0xFFFFFFFF80000000ul); // t1
    }
}
