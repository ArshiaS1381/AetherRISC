using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.Pipeline.Controller;
using Xunit;
using AetherRISC.Core.Architecture.Hardware.Memory;
using AetherRISC.Core.Helpers;
using AetherRISC.Core.Architecture.Hardware.ISA.Encoding;

namespace AetherRISC.Core.Tests.Integration;

public class AlgorithmTests
{
    private (MachineState state, PipelineController pipeline, TestAssembler asm) Setup()
    {
        var state = new MachineState(SystemConfig.Rv64());
        state.Memory = new SystemBus(4096);
        state.ProgramCounter = 0;
        return (state, new PipelineController(state), new TestAssembler());
    }

    private void LoadAndRun(MachineState state, PipelineController pipeline, TestAssembler asm, int cycles)
    {
        var insts = asm.Assemble();
        for (int i = 0; i < insts.Count; i++)
            state.Memory!.WriteWord((uint)(i * 4), InstructionEncoder.Encode(insts[i]));
        for (int i = 0; i < cycles; i++) pipeline.Cycle();
    }

    [Fact]
    public void SimpleCountdown_From5To0_Fixed()
    {
        var (state, pipeline, asm) = Setup();

        asm.Add(pc => Inst.Addi(10, 0, 5));                    // 0x00: x10 = 5
        asm.Add(pc => Inst.Beq(10, 0, asm.To("exit", pc)), "loop"); // 0x04: if x10 == 0, goto exit
        asm.Add(pc => Inst.Addi(10, 10, -1));                  // 0x08: x10--
        asm.Add(pc => Inst.Jal(0, asm.To("loop", pc)));        // 0x0C: goto loop
        asm.Add(pc => Inst.Addi(11, 0, 99), "exit");           // 0x10: exit marker
        asm.Add(pc => Inst.Nop());
        asm.Add(pc => Inst.Nop());

        LoadAndRun(state, pipeline, asm, 120);

        Assert.Equal((ulong)0, state.Registers.Read(10));
        Assert.Equal((ulong)99, state.Registers.Read(11));
    }

    [Fact]
    public void SumFrom1To5()
    {
        // x10 = sum (starts 0), x11 = counter (starts 5)
        // Result: 1+2+3+4+5 = 15
        var (state, pipeline, asm) = Setup();

        asm.Add(pc => Inst.Addi(10, 0, 0));                      // x10 = 0 (sum)
        asm.Add(pc => Inst.Addi(11, 0, 5));                      // x11 = 5 (counter)
        asm.Add(pc => Inst.Add(10, 10, 11), "loop");             // sum += counter
        asm.Add(pc => Inst.Addi(11, 11, -1));                    // counter--
        asm.Add(pc => Inst.Bne(11, 0, asm.To("loop", pc)));      // if counter != 0, loop
        asm.Add(pc => Inst.Nop());
        asm.Add(pc => Inst.Nop());

        LoadAndRun(state, pipeline, asm, 100);

        Assert.Equal((ulong)15, state.Registers.Read(10));
        Assert.Equal((ulong)0, state.Registers.Read(11));
    }

    [Fact]
    public void Factorial3()
    {
        // 3! = 6
        var (state, pipeline, asm) = Setup();

        asm.Add(pc => Inst.Addi(10, 0, 1));                       // x10 = 1 (result)
        asm.Add(pc => Inst.Addi(11, 0, 3));                       // x11 = 3 (i)
        asm.Add(pc => Inst.Addi(12, 0, 2), "loop");               // x12 = 2 (threshold)
        asm.Add(pc => Inst.Blt(11, 12, asm.To("exit", pc)));      // if i < 2, exit
        asm.Add(pc => Inst.Mul(10, 10, 11));                      // result *= i
        asm.Add(pc => Inst.Addi(11, 11, -1));                     // i--
        asm.Add(pc => Inst.Jal(0, asm.To("loop", pc)));           // goto loop
        asm.Add(pc => Inst.Nop(), "exit");
        asm.Add(pc => Inst.Nop());

        LoadAndRun(state, pipeline, asm, 100);

        Assert.Equal((ulong)6, state.Registers.Read(10));
    }

    [Fact]
    public void Factorial5()
    {
        // 5! = 120
        var (state, pipeline, asm) = Setup();

        asm.Add(pc => Inst.Addi(10, 0, 1));                       // x10 = 1 (result)
        asm.Add(pc => Inst.Addi(11, 0, 5));                       // x11 = 5 (i)
        asm.Add(pc => Inst.Addi(12, 0, 2), "loop");               // x12 = 2 (threshold)
        asm.Add(pc => Inst.Blt(11, 12, asm.To("exit", pc)));      // if i < 2, exit
        asm.Add(pc => Inst.Mul(10, 10, 11));                      // result *= i
        asm.Add(pc => Inst.Addi(11, 11, -1));                     // i--
        asm.Add(pc => Inst.Jal(0, asm.To("loop", pc)));           // goto loop
        asm.Add(pc => Inst.Nop(), "exit");
        asm.Add(pc => Inst.Nop());

        LoadAndRun(state, pipeline, asm, 150);

        Assert.Equal((ulong)120, state.Registers.Read(10));
    }

    [Fact]
    public void Fibonacci_6th_Is_8()
    {
        // Fib(6) = 8: sequence 0,1,1,2,3,5,8
        var (state, pipeline, asm) = Setup();

        asm.Add(pc => Inst.Addi(10, 0, 0));                       // x10 = 0 (fib n-2)
        asm.Add(pc => Inst.Addi(11, 0, 1));                       // x11 = 1 (fib n-1)
        asm.Add(pc => Inst.Addi(12, 0, 5));                       // x12 = 5 (iterations)
        asm.Add(pc => Inst.Beq(12, 0, asm.To("exit", pc)), "loop"); // if counter == 0, exit
        asm.Add(pc => Inst.Add(13, 10, 11));                      // x13 = fib(n-2) + fib(n-1)
        asm.Add(pc => Inst.Add(10, 11, 0));                       // x10 = x11
        asm.Add(pc => Inst.Add(11, 13, 0));                       // x11 = x13
        asm.Add(pc => Inst.Addi(12, 12, -1));                     // counter--
        asm.Add(pc => Inst.Jal(0, asm.To("loop", pc)));           // goto loop
        asm.Add(pc => Inst.Nop(), "exit");
        asm.Add(pc => Inst.Nop());

        LoadAndRun(state, pipeline, asm, 150);

        Assert.Equal((ulong)8, state.Registers.Read(11));
    }

    [Fact]
    public void Fibonacci_5th_Is_5()
    {
        // Fib(5) = 5: iterate 4 times from (0,1)
        var (state, pipeline, asm) = Setup();

        asm.Add(pc => Inst.Addi(10, 0, 0));
        asm.Add(pc => Inst.Addi(11, 0, 1));
        asm.Add(pc => Inst.Addi(12, 0, 4));                       // 4 iterations for Fib(5)
        asm.Add(pc => Inst.Beq(12, 0, asm.To("exit", pc)), "loop");
        asm.Add(pc => Inst.Add(13, 10, 11));
        asm.Add(pc => Inst.Add(10, 11, 0));
        asm.Add(pc => Inst.Add(11, 13, 0));
        asm.Add(pc => Inst.Addi(12, 12, -1));
        asm.Add(pc => Inst.Jal(0, asm.To("loop", pc)));
        asm.Add(pc => Inst.Nop(), "exit");

        LoadAndRun(state, pipeline, asm, 120);

        Assert.Equal((ulong)5, state.Registers.Read(11));
    }

    [Fact]
    public void Max_Of_Two_Numbers()
    {
        // max(7, 12) = 12
        var (state, pipeline, asm) = Setup();

        asm.Add(pc => Inst.Addi(10, 0, 7));
        asm.Add(pc => Inst.Addi(11, 0, 12));
        asm.Add(pc => Inst.Nop());
        asm.Add(pc => Inst.Nop());
        asm.Add(pc => Inst.Bge(10, 11, asm.To("x10_bigger", pc)));
        asm.Add(pc => Inst.Add(12, 11, 0));                       // x12 = x11
        asm.Add(pc => Inst.Jal(0, asm.To("done", pc)));
        asm.Add(pc => Inst.Add(12, 10, 0), "x10_bigger");         // x12 = x10
        asm.Add(pc => Inst.Nop(), "done");
        asm.Add(pc => Inst.Nop());

        LoadAndRun(state, pipeline, asm, 40);

        Assert.Equal((ulong)12, state.Registers.Read(12));
    }

    [Fact]
    public void Min_Of_Two_Numbers()
    {
        // min(7, 12) = 7
        var (state, pipeline, asm) = Setup();

        asm.Add(pc => Inst.Addi(10, 0, 7));
        asm.Add(pc => Inst.Addi(11, 0, 12));
        asm.Add(pc => Inst.Nop());
        asm.Add(pc => Inst.Nop());
        asm.Add(pc => Inst.Blt(10, 11, asm.To("x10_smaller", pc)));
        asm.Add(pc => Inst.Add(12, 11, 0));
        asm.Add(pc => Inst.Jal(0, asm.To("done", pc)));
        asm.Add(pc => Inst.Add(12, 10, 0), "x10_smaller");
        asm.Add(pc => Inst.Nop(), "done");
        asm.Add(pc => Inst.Nop());

        LoadAndRun(state, pipeline, asm, 40);

        Assert.Equal((ulong)7, state.Registers.Read(12));
    }

    [Fact]
    public void Absolute_Value_Negative()
    {
        // abs(-5) = 5
        var (state, pipeline, asm) = Setup();

        asm.Add(pc => Inst.Addi(10, 0, -5));                      // x10 = -5
        asm.Add(pc => Inst.Nop());
        asm.Add(pc => Inst.Nop());
        asm.Add(pc => Inst.Bge(10, 0, asm.To("done", pc)));       // if x10 >= 0, skip
        asm.Add(pc => Inst.Sub(10, 0, 10));                       // x10 = 0 - x10
        asm.Add(pc => Inst.Nop(), "done");
        asm.Add(pc => Inst.Nop());

        LoadAndRun(state, pipeline, asm, 30);

        Assert.Equal((ulong)5, state.Registers.Read(10));
    }

    [Fact]
    public void Absolute_Value_Positive()
    {
        // abs(5) = 5 (no change)
        var (state, pipeline, asm) = Setup();

        asm.Add(pc => Inst.Addi(10, 0, 5));
        asm.Add(pc => Inst.Nop());
        asm.Add(pc => Inst.Nop());
        asm.Add(pc => Inst.Bge(10, 0, asm.To("done", pc)));
        asm.Add(pc => Inst.Sub(10, 0, 10));
        asm.Add(pc => Inst.Nop(), "done");
        asm.Add(pc => Inst.Nop());

        LoadAndRun(state, pipeline, asm, 30);

        Assert.Equal((ulong)5, state.Registers.Read(10));
    }

    [Fact]
    public void GCD_Euclidean()
    {
        // GCD(48, 18) = 6
        // Algorithm: while b != 0: t = b; b = a % b; a = t
        // FIX: Initialize working copy OUTSIDE the mod loop
        var (state, pipeline, asm) = Setup();

        asm.Add(pc => Inst.Addi(10, 0, 48));                      // x10 = a = 48
        asm.Add(pc => Inst.Addi(11, 0, 18));                      // x11 = b = 18
        asm.Add(pc => Inst.Beq(11, 0, asm.To("done", pc)), "loop"); // if b == 0, done
        asm.Add(pc => Inst.Add(12, 11, 0));                       // t = b (save for later)
        asm.Add(pc => Inst.Add(13, 10, 0));                       // x13 = a (working copy) - ONCE!
        asm.Add(pc => Inst.Blt(13, 11, asm.To("mod_done", pc)), "mod_loop"); // if x13 < b, mod done
        asm.Add(pc => Inst.Sub(13, 13, 11));                      // x13 -= b
        asm.Add(pc => Inst.Jal(0, asm.To("mod_loop", pc)));       // continue mod loop
        asm.Add(pc => Inst.Add(11, 13, 0), "mod_done");           // b = a % b (x13)
        asm.Add(pc => Inst.Add(10, 12, 0));                       // a = t
        asm.Add(pc => Inst.Jal(0, asm.To("loop", pc)));           // next GCD iteration
        asm.Add(pc => Inst.Nop(), "done");
        asm.Add(pc => Inst.Nop());

        LoadAndRun(state, pipeline, asm, 500);

        Assert.Equal((ulong)6, state.Registers.Read(10));
    }

    [Fact]
    public void Power_2_To_10()
    {
        // 2^10 = 1024
        var (state, pipeline, asm) = Setup();

        asm.Add(pc => Inst.Addi(10, 0, 1));                       // result = 1
        asm.Add(pc => Inst.Addi(11, 0, 2));                       // base = 2
        asm.Add(pc => Inst.Addi(12, 0, 10));                      // exponent = 10
        asm.Add(pc => Inst.Beq(12, 0, asm.To("done", pc)), "loop");
        asm.Add(pc => Inst.Mul(10, 10, 11));                      // result *= base
        asm.Add(pc => Inst.Addi(12, 12, -1));                     // exp--
        asm.Add(pc => Inst.Jal(0, asm.To("loop", pc)));
        asm.Add(pc => Inst.Nop(), "done");

        LoadAndRun(state, pipeline, asm, 200);

        Assert.Equal((ulong)1024, state.Registers.Read(10));
    }

    [Fact]
    public void IsEven_True()
    {
        // Check if 42 is even (42 & 1 == 0)
        var (state, pipeline, asm) = Setup();

        asm.Add(pc => Inst.Addi(10, 0, 42));
        asm.Add(pc => Inst.Andi(11, 10, 1));                      // x11 = x10 & 1
        asm.Add(pc => Inst.Nop());
        asm.Add(pc => Inst.Nop());
        asm.Add(pc => Inst.Nop());

        LoadAndRun(state, pipeline, asm, 20);

        Assert.Equal((ulong)0, state.Registers.Read(11));         // 0 means even
    }

    [Fact]
    public void IsEven_False()
    {
        // Check if 43 is even
        var (state, pipeline, asm) = Setup();

        asm.Add(pc => Inst.Addi(10, 0, 43));
        asm.Add(pc => Inst.Andi(11, 10, 1));
        asm.Add(pc => Inst.Nop());
        asm.Add(pc => Inst.Nop());
        asm.Add(pc => Inst.Nop());

        LoadAndRun(state, pipeline, asm, 20);

        Assert.Equal((ulong)1, state.Registers.Read(11));         // 1 means odd
    }

    [Fact]
    public void CountBits_Simple()
    {
        // Count set bits in 0b1011 (3 bits set)
        var (state, pipeline, asm) = Setup();

        asm.Add(pc => Inst.Addi(10, 0, 0b1011));                  // x10 = input
        asm.Add(pc => Inst.Addi(11, 0, 0));                       // x11 = count
        asm.Add(pc => Inst.Beq(10, 0, asm.To("done", pc)), "loop");
        asm.Add(pc => Inst.Andi(12, 10, 1));                      // x12 = x10 & 1
        asm.Add(pc => Inst.Add(11, 11, 12));                      // count += bit
        asm.Add(pc => Inst.Srli(10, 10, 1));                      // x10 >>= 1
        asm.Add(pc => Inst.Jal(0, asm.To("loop", pc)));
        asm.Add(pc => Inst.Nop(), "done");

        LoadAndRun(state, pipeline, asm, 100);

        Assert.Equal((ulong)3, state.Registers.Read(11));
    }
}
