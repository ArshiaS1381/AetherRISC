using Xunit;
using AetherRISC.Tests.Infrastructure;
using AetherRISC.Core.Assembler;

namespace AetherRISC.Tests.Integration.Assembler
{
    public class PseudoExpansionTests : CpuTestFixture
    {
        [Fact]
        public void Li_Expands_Correctly()
        {
            Init64();
            var asm = new SourceAssembler("li x1, 0x12345678");
            asm.Assemble(Machine);
            
            base.Run(5);
            AssertReg(1, 0x12345678);
        }

        [Fact]
        public void Mv_Expands_Correctly()
        {
            Init64();
            var asm = new SourceAssembler("li x1, 10\nmv x2, x1");
            asm.Assemble(Machine);
            
            base.Run(10);
            AssertReg(2, 10);
        }
    }
}
