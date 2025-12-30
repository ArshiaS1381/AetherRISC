using Xunit;
using AetherRISC.Tests.Infrastructure;
using AetherRISC.Core.Assembler;

namespace AetherRISC.Tests.Integration.Programs
{
    public class ComplexExecutionTests : CpuTestFixture
    {
        [Fact]
        public void Loop_Summation()
        {
            // sum = 0; for(i=10; i>0; i--) sum += i;
            Init64();
            var source = @"
                li x1, 10   # i
                li x2, 0    # sum
                loop:
                add x2, x2, x1
                addi x1, x1, -1
                bnez x1, loop
                ebreak
            ";
            
            var asm = new SourceAssembler(source);
            asm.Assemble(Machine);
            
            base.Run(100);
            
            AssertReg(2, 55);
        }
    }
}
