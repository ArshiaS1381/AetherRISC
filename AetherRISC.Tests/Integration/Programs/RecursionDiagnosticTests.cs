using Xunit;
using Xunit.Abstractions;
using AetherRISC.Tests.Infrastructure;
using AetherRISC.Core.Helpers;
using AetherRISC.Core.Architecture.Simulation;

namespace AetherRISC.Tests.Integration.Programs;

public class RecursionDiagnosticTests : CpuTestFixture
{
    private readonly ITestOutputHelper _output;
    
    public RecursionDiagnosticTests(ITestOutputHelper output) => _output = output;

    [Fact]
    public void Diag_Factorial_2_Levels()
    {
        // fact(2) = 2 * fact(1) = 2 * 1 = 2
        Init64();
        Machine.Host = new MultiOSHandler { Silent = true };
        Machine.Registers.Write(2, 0x100000);

        string code = @"
            .text
            li a0, 2
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
        Runner.Run(50);

        ulong result = Machine.Registers.Read(10);
        _output.WriteLine($"fact(2) = {result} (expected 2)");
        AssertReg(10, 2ul);
    }

    [Fact]
    public void Diag_Factorial_3_Levels()
    {
        // fact(3) = 3 * 2 * 1 = 6
        Init64();
        Machine.Host = new MultiOSHandler { Silent = true };
        Machine.Registers.Write(2, 0x100000);

        string code = @"
            .text
            li a0, 3
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
        Runner.Run(100);

        ulong result = Machine.Registers.Read(10);
        _output.WriteLine($"fact(3) = {result} (expected 6)");
        AssertReg(10, 6ul);
    }

    [Fact]
    public void Diag_Factorial_4_Levels()
    {
        // fact(4) = 24
        Init64();
        Machine.Host = new MultiOSHandler { Silent = true };
        Machine.Registers.Write(2, 0x100000);

        string code = @"
            .text
            li a0, 4
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
        Runner.Run(150);

        ulong result = Machine.Registers.Read(10);
        _output.WriteLine($"fact(4) = {result} (expected 24)");
        AssertReg(10, 24ul);
    }

    [Fact]
    public void Diag_Stack_Frame_Preservation()
    {
        // Verify stack frame values survive a nested call
        Init64();
        Machine.Registers.Write(2, 0x100000);

        string code = @"
            .text
            li a0, 42
            addi sp, sp, -16
            sd a0, 0(sp)
            jal ra, dummy
            ld a1, 0(sp)
            addi sp, sp, 16
            ebreak

            dummy:
                addi sp, sp, -16
                li t0, 999
                sd t0, 0(sp)
                addi sp, sp, 16
                ret
        ";

        var asm = new SourceAssembler(code) { TextBase = 0 };
        asm.Assemble(Machine);
        Machine.ProgramCounter = 0;
        Runner.Run(20);

        ulong result = Machine.Registers.Read(11);
        _output.WriteLine($"a1 = {result} (expected 42 - preserved across call)");
        AssertReg(11, 42ul);
    }
}
