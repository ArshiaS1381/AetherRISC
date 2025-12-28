using Xunit;
using AetherRISC.Tests.Infrastructure;
using AetherRISC.Core.Helpers;
using AetherRISC.Core.Architecture.Simulation;

namespace AetherRISC.Tests.Integration.Programs;

public class ComplexExecutionTests : CpuTestFixture
{
    [Fact]
    public void Recursive_Factorial_Stack_Torture()
    {
        Init64();
        Machine.Host = new MultiOSHandler { Silent = true };
        Machine.Registers.Write(2, 0x100000); // Initialize Stack Pointer (SP)

        // Recursive Factorial of 5
        // Uses JAL, JALR (RET), Stack Pushes/Pops
        string code = @"
            .text
            li a0, 5
            jal ra, fact
            ebreak

            fact:
                addi sp, sp, -16
                sd ra, 8(sp)
                sd a0, 0(sp)
                addi t1, zero, 1
                bgt a0, t1, recurse
                li a0, 1
                addi sp, sp, 16
                ret
            recurse:
                addi a0, a0, -1
                jal ra, fact
                ld t1, 0(sp)
                mul a0, a0, t1
                ld ra, 8(sp)
                addi sp, sp, 16
                ret
        ";

        var asm = new SourceAssembler(code) { TextBase = 0 };
        asm.Assemble(Machine);
        Machine.ProgramCounter = 0;

        Runner.Run(200);

        // 5! = 120
        AssertReg(10, 120ul);
    }
}
