using Xunit;
using Xunit.Abstractions;
using AetherRISC.Tests.Infrastructure;
using AetherRISC.Core.Assembler;

namespace AetherRISC.Tests.Integration.Programs
{
    public class FactorialDiagnosticTests : CpuTestFixture
    {
        private readonly ITestOutputHelper _output;
        public FactorialDiagnosticTests(ITestOutputHelper output) => _output = output;

        [Fact]
        public void Iterative_Factorial_5()
        {
            Init64();
            var source = @"
                li x10, 5
                li x11, 1
                loop:
                mul x11, x11, x10
                addi x10, x10, -1
                bgtz x10, loop
                ebreak
            ";
            
            var asm = new SourceAssembler(source);
            asm.Assemble(Machine);
            
            base.Run(50);
            AssertReg(11, 120);
        }

        [Fact]
        public void Recursive_Factorial_5()
        {
            Init64();
            var source = @"
                .text
                li x10, 5
                li sp, 0x1000
                call fact
                ebreak

                fact:
                addi sp, sp, -16
                sd ra, 8(sp)
                sd x10, 0(sp)
                
                li t0, 1
                ble x10, t0, base_case
                
                addi x10, x10, -1
                call fact
                
                ld t1, 0(sp)   # t1 = n
                mul x10, x10, t1
                j ret

                base_case:
                li x10, 1
                
                ret:
                ld ra, 8(sp)
                addi sp, sp, 16
                ret
            ";
            
            var asm = new SourceAssembler(source);
            asm.Assemble(Machine);
            
            base.Run(200);
            AssertReg(10, 120);
        }
    }
}
