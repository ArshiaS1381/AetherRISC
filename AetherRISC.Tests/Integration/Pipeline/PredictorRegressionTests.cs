using Xunit;
using Xunit.Abstractions;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Simulation.Runners;
using AetherRISC.Core.Assembler;
using AetherRISC.Core.Architecture.Hardware.Memory;
using AetherRISC.Core.Architecture.Simulation; 
using AetherRISC.Tests.Infrastructure;
using System.IO;
using System.Collections.Generic;

namespace AetherRISC.Tests.Integration.Pipeline;

public class PredictorRegressionTests
{
    private readonly ITestOutputHelper _output;

    public PredictorRegressionTests(ITestOutputHelper output) => _output = output;

    private void AssertCorrectExecution(string asm, ulong expectedResult, int resultReg = 10)
    {
        var predictors = new[] { "static", "bimodal", "gshare" };

        foreach (var pred in predictors)
        {
            _output.WriteLine($"Testing Predictor: {pred.ToUpper()}");

            var config = SystemConfig.Rv64();
            var state = new MachineState(config);
            state.Memory = new SystemBus(1024 * 1024);
            state.Host = new MultiOSHandler { Output = new StringWriter(), Silent = true };

            var assembler = new SourceAssembler(asm) { TextBase = 0x80000000 };
            assembler.Assemble(state);
            
            // CRITICAL FIX: Explicitly set the Register PC.
            // Some pipeline implementations read Registers[PC_INDEX] directly, ignoring the property wrapper.
            state.ProgramCounter = 0x80000000;
            state.Registers.PC = 0x80000000;

            var runner = new PipelinedRunner(state, new NullLogger(), pred);
            
            int cycles = 0;
            // Limit set to 250k to accommodate Gshare learning curve on recursion
            while (!state.Halted && cycles < 250000) 
            {
                runner.Step(cycles++);
            }

            if (cycles >= 250000) 
            {
                _output.WriteLine($"WARNING: Simulation timed out! PC: {state.Registers.PC:X}");
            }

            ulong actual = state.Registers.Read(resultReg);
            
            Assert.True(state.Halted, $"Predictor {pred} failed to halt at PC {state.Registers.PC:X}");
            Assert.Equal(expectedResult, actual);
        }
    }

    [Fact]
    public void Regression_RecursiveFibonacci()
    {
        var asm = @"
            .text
            .globl _start
            _start:
                li a0, 7        # Calculate Fib(7) = 13
                jal ra, fib
                ebreak

            fib:
                addi sp, sp, -16
                sd ra, 8(sp)
                sd s0, 0(sp)
                
                li t0, 1
                ble a0, t0, fib_base

                mv s0, a0
                addi a0, a0, -1
                jal ra, fib
                
                mv t1, a0
                sd t1, 0(sp)

                addi a0, s0, -2
                jal ra, fib
                
                ld t1, 0(sp)
                add a0, a0, t1
                j fib_ret

            fib_base:
            fib_ret:
                ld ra, 8(sp)
                ld s0, 0(sp)
                addi sp, sp, 16
                ret
        ";
        AssertCorrectExecution(asm, 13);
    }

    [Fact]
    public void Regression_NestedLoop_Summation()
    {
        var asm = @"
            .text
            li s0, 0        
            li s1, 0        

        outer:
            li t0, 5
            bge s1, t0, done
            li s2, 0        

        inner:
            li t0, 5
            bge s2, t0, next_outer
            
            mul t1, s1, s2
            add s0, s0, t1
            
            addi s2, s2, 1
            j inner

        next_outer:
            addi s1, s1, 1
            j outer

        done:
            mv a0, s0
            ebreak
        ";
        AssertCorrectExecution(asm, 100);
    }

    [Fact]
    public void Regression_SwitchCase_JalrTable()
    {
        var asm = @"
            .data
            jumptable:
                .word case0, case1, case2, case3

            .text
            li s0, 0
            li s1, 0

        loop:
            li t0, 4
            bge s1, t0, done

            la t1, jumptable
            slli t2, s1, 2
            add t1, t1, t2
            lw t3, 0(t1)
            
            jalr ra, t3, 0

        back:
            addi s1, s1, 1
            j loop

        case0:
            addi s0, s0, 10
            j back
        case1:
            addi s0, s0, 20
            j back
        case2:
            addi s0, s0, 30
            j back
        case3:
            addi s0, s0, 40
            j back

        done:
            mv a0, s0
            ebreak
        ";
        AssertCorrectExecution(asm, 100);
    }
}
