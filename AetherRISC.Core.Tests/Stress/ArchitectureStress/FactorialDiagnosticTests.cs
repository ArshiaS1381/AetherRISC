using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Simulation.Runners;
using AetherRISC.Core.Architecture.Simulation;
using AetherRISC.Core.Helpers;
using AetherRISC.Core.Architecture.Hardware.Memory;
using Xunit;
using Xunit.Abstractions;

namespace AetherRISC.Core.Tests.Architecture.Stress;

public class FactorialDiagnosticTests
{
    private readonly ITestOutputHelper _output;
    public FactorialDiagnosticTests(ITestOutputHelper output) => _output = output;

    [Fact]
    public void BGT_Pseudo_Works()
    {
        var state = new MachineState(SystemConfig.Rv64());
        state.Memory = new SystemBus(0x10000);
        state.Host = new MultiOSHandler { Silent = true };

        string code = @"
            .text
            li a0, 5
            li t0, 1
            bgt a0, t0, taken
            li a1, 0
            j done
        taken:
            li a1, 1
        done:
            ebreak
        ";

        var asm = new SourceAssembler(code) { TextBase = 0 };
        asm.Assemble(state);
        state.ProgramCounter = 0;

        new PipelinedRunner(state, new NullLogger()).Run(50);

        _output.WriteLine($"a0 = {state.Registers.Read(10)}");
        _output.WriteLine($"a1 = {state.Registers.Read(11)}");

        Assert.Equal(5ul, state.Registers.Read(10));
        Assert.Equal(1ul, state.Registers.Read(11)); // Should be 1 if branch taken
    }

    [Fact]
    public void SD_LD_Stack_Works()
    {
        var state = new MachineState(SystemConfig.Rv64());
        state.Memory = new SystemBus(0x10000);
        state.Host = new MultiOSHandler { Silent = true };
        state.Registers.Write(2, 0x8000); // sp

        string code = @"
            .text
            li a0, 42
            addi sp, sp, -16
            sd a0, 0(sp)
            li a0, 0
            ld a1, 0(sp)
            addi sp, sp, 16
            ebreak
        ";

        var asm = new SourceAssembler(code) { TextBase = 0 };
        asm.Assemble(state);
        state.ProgramCounter = 0;

        new PipelinedRunner(state, new NullLogger()).Run(50);

        _output.WriteLine($"a0 = {state.Registers.Read(10)}");
        _output.WriteLine($"a1 = {state.Registers.Read(11)}");

        Assert.Equal(0ul, state.Registers.Read(10));
        Assert.Equal(42ul, state.Registers.Read(11));
    }

    [Fact]
    public void JAL_And_RET_Work()
    {
        var state = new MachineState(SystemConfig.Rv64());
        state.Memory = new SystemBus(0x10000);
        state.Host = new MultiOSHandler { Silent = true };
        state.Registers.Write(2, 0x8000); // sp

        string code = @"
            .text
            li a0, 5
            jal ra, myfunc
            mv s0, a0
            ebreak

        myfunc:
            addi a0, a0, 10
            ret
        ";

        var asm = new SourceAssembler(code) { TextBase = 0 };
        asm.Assemble(state);
        state.ProgramCounter = 0;

        new PipelinedRunner(state, new NullLogger()).Run(50);

        _output.WriteLine($"a0 = {state.Registers.Read(10)}");
        _output.WriteLine($"s0 = {state.Registers.Read(8)}");

        Assert.Equal(15ul, state.Registers.Read(8)); // 5 + 10
    }

    [Fact]
    public void Simple_Iterative_Factorial()
    {
        var state = new MachineState(SystemConfig.Rv64());
        state.Memory = new SystemBus(0x10000);
        state.Host = new MultiOSHandler { Silent = true };

        // Iterative factorial: result = 1; for(i=n; i>1; i--) result *= i
        string code = @"
            .text
            li a0, 5       # n = 5
            li a1, 1       # result = 1
        loop:
            li t0, 1
            ble a0, t0, done
            mul a1, a1, a0
            addi a0, a0, -1
            j loop
        done:
            mv s0, a1
            ebreak
        ";

        var asm = new SourceAssembler(code) { TextBase = 0 };
        asm.Assemble(state);
        state.ProgramCounter = 0;

        new PipelinedRunner(state, new NullLogger()).Run(200);

        _output.WriteLine($"s0 = {state.Registers.Read(8)}");

        Assert.Equal(120ul, state.Registers.Read(8)); // 5! = 120
    }
}
