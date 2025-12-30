using Xunit;
using Xunit.Abstractions;
using AetherRISC.Tests.Infrastructure;
using AetherRISC.Core.Assembler;

namespace AetherRISC.Tests.Integration.Programs
{
    public class RecursionDiagnosticTests : CpuTestFixture
    {
        private readonly ITestOutputHelper _output;
        public RecursionDiagnosticTests(ITestOutputHelper output) => _output = output;

        [Fact]
        public void Recursive_Fibonacci()
        {
            Init64();
            // Simple recursive fib code structure
            var source = @"
            .text
            li x10, 5
            jal ra, fib
            ebreak

            fib:
            addi sp, sp, -16
            sd ra, 8(sp)
            sd x10, 0(sp)
            li t0, 2
            blt x10, t0, ret_base
            
            addi x10, x10, -1
            jal ra, fib
            mv t1, x10    # t1 = fib(n-1)
            
            ld x10, 0(sp)
            addi x10, x10, -2
            sd t1, 0(sp)  # save fib(n-1)
            jal ra, fib   # x10 = fib(n-2)
            
            ld t1, 0(sp)  # restore fib(n-1)
            add x10, x10, t1
            j ret

            ret_base:
            # x10 is already n (0 or 1)

            ret:
            ld ra, 8(sp)
            addi sp, sp, 16
            ret
            ";

            var asm = new SourceAssembler(source);
            asm.Assemble(Machine);
            Machine.Registers.Write(2, 0x10000); // SP

            base.Run(1000);
            
            AssertReg(10, 5); // Fib(5) = 5
        }
    }
}
