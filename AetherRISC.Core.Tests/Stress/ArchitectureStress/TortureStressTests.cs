using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Simulation.Runners;
using AetherRISC.Core.Architecture.Simulation;
using AetherRISC.Core.Helpers;
using AetherRISC.Core.Architecture.Hardware.Memory;
using Xunit;

namespace AetherRISC.Core.Tests.Architecture.Stress;

public class TortureStressTests
{
    [Fact]
    public void Multiplication_Torture_Test()
    {
        var state = new MachineState(SystemConfig.Rv64());
        state.Memory = new SystemBus(0x1000000);
        state.Host = new MultiOSHandler { Silent = true };

        // Testing sign extension and large multiplications
        string code = @"
            .text
            li t0, -1          # 0xFFFFFFFFFFFFFFFF
            li t1, 5
            mul t2, t0, t1     # -1 * 5 = -5
            li t3, 0x7FFFFFFF  # Max Int32
            mul t4, t3, t3     # Large positive
            ebreak
        ";

        new SourceAssembler(code).Assemble(state);
        new PipelinedRunner(state, new NullLogger()).Run(100);

        Assert.Equal(unchecked((ulong)-5), state.Registers.Read(7)); // t2
    }

    [Fact]
    public void Zero_Register_Persistence_Test()
    {
        var state = new MachineState(SystemConfig.Rv64());
        state.Memory = new SystemBus(0x1000000);
        state.Host = new MultiOSHandler { Silent = true };

        // Attempting to write to x0 (zero)
        string code = @"
            .text
            li x0, 999
            addi x0, x0, 50
            mul x0, x1, x2
            ebreak
        ";

        new SourceAssembler(code).Assemble(state);
        new PipelinedRunner(state, new NullLogger()).Run(50);

        Assert.Equal(0ul, state.Registers.Read(0)); 
    }

    [Fact]
    public void Recursive_Factorial_Integration_Test()
    {
        var state = new MachineState(SystemConfig.Rv64());
        state.Memory = new SystemBus(0x1000000);
        state.Host = new MultiOSHandler { Silent = true };
        state.Registers.Write(2, 0x800000); // Set Stack Pointer

        // Standard Recursive Factorial (testing JAL, stack management, branches)
        string code = @"
            .text
            li a0, 5
            jal ra, fact
            mv s0, a0
            ebreak

            fact:
                addi sp, sp, -16
                sd ra, 8(sp)
                sd a0, 0(sp)
                li t0, 1
                bgt a0, t0, continue
                li a0, 1
                addi sp, sp, 16
                ret
            continue:
                addi a0, a0, -1
                jal ra, fact
                ld t1, 0(sp)
                mul a0, a0, t1
                ld ra, 8(sp)
                addi sp, sp, 16
                ret
        ";

        new SourceAssembler(code).Assemble(state);
        new PipelinedRunner(state, new NullLogger()).Run(1000);

        Assert.Equal(120ul, state.Registers.Read(8)); // s0 should be 5! = 120
    }
}
