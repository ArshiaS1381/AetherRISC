using System;
using System.Linq;
using AetherRISC.Core.Architecture.Hardware.Memory;
using AetherRISC.Core.Architecture.Simulation;
using AetherRISC.Core.Architecture.Simulation.Runners;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Helpers;
using Xunit;

namespace AetherRISC.Core.Tests.Architecture.Stress;

public class UltraStressSuiteTests
{
    private static MachineState NewState(
        uint memSize = 0x20000,
        uint textBase = 0,
        uint dataBase = 0x8000
    )
    {
        var state = new MachineState(SystemConfig.Rv64());
        state.Memory = new SystemBus(memSize);
        state.Host = new MultiOSHandler { Silent = true, Kind = OSKind.Linux };
        state.ProgramCounter = textBase;

        return state;
    }

    private static void RunAsm(
        MachineState state,
        string asm,
        int maxCycles = 20000,
        uint textBase = 0,
        uint dataBase = 0x8000
    )
    {
        var assembler = new SourceAssembler(asm)
        {
            TextBase = textBase,
            DataBase = dataBase
        };
        assembler.Assemble(state);

        // Ensure PC starts at assembled text base (assembler sets it too, but
        // be explicit for stress tests).
        state.ProgramCounter = textBase;

        new PipelinedRunner(state, new NullLogger()).Run(maxCycles);
    }

    [Fact]
    public void Stress_Arithmetic_Branching_And_Dependency_Chains()
    {
        var s = NewState(memSize: 0x20000, textBase: 0, dataBase: 0x8000);

        // Computes:
        // - sum1 = 1+2+...+2000
        // - fact10 = 10!
        // Uses lots of dependent chains and branches.
        string code = @"
            .text
            li s0, 0          # sum
            li t0, 1          # i
            li t1, 2000       # N

        sum_loop:
            add s0, s0, t0
            addi t0, t0, 1
            ble t0, t1, sum_loop

            # factorial 10
            li s1, 1          # fact
            li t2, 10         # n
            li t3, 2          # i=2

        fac_loop:
            mul s1, s1, t3
            addi t3, t3, 1
            ble t3, t2, fac_loop

            # Mix in a long dependency chain (forwarding pressure)
            mv a0, s0
            addi a0, a0, 7
            addi a0, a0, -3
            addi a0, a0, 11
            addi a0, a0, -9
            addi a0, a0, 13
            addi a0, a0, -19
            addi a0, a0, 23

            ebreak
        ";

        RunAsm(s, code, maxCycles: 20000, textBase: 0, dataBase: 0x8000);

        ulong sumExpected = (ulong)(2000 * 2001 / 2);
        Assert.Equal(sumExpected, s.Registers.Read(8)); // s0
        Assert.Equal(3628800ul, s.Registers.Read(9)); // s1
        Assert.Equal(sumExpected + 23ul, s.Registers.Read(10)); // a0
    }

    [Fact]
    public void Stress_DataSection_LabelAddressing_Copy_And_Checksum()
    {
        var s = NewState(memSize: 0x40000, textBase: 0, dataBase: 0x20000);

        // Copies 16 dwords from src -> dst and computes XOR checksum of dst.
        // Exercises: .data, .word, labels, LA/LI, loads/stores, loops, branches.
        string code = @"
            .data
        src:
            .word 0x11111111 0x22222222 0x33333333 0x44444444
            .word 0xAAAAAAAA 0xBBBBBBBB 0xCCCCCCCC 0xDDDDDDDD
            .word 0x01234567 0x89ABCDEF 0x0BADF00D 0xFEEDFACE
            .word 0x00000000 0x7FFFFFFF 0x80000000 0xFFFFFFFF

        dst:
            .word 0 0 0 0
            .word 0 0 0 0
            .word 0 0 0 0
            .word 0 0 0 0

            .text
            la s0, src        # s0 = &src
            la s1, dst        # s1 = &dst
            li s2, 16         # count (words)
            li s3, 0          # index bytes
            li s4, 0          # checksum

        copy_loop:
            lw t0, 0(s0)
            sw t0, 0(s1)
            xor s4, s4, t0
            addi s0, s0, 4
            addi s1, s1, 4
            addi s3, s3, 1
            blt s3, s2, copy_loop

            ebreak
        ";

        RunAsm(s, code, maxCycles: 20000, textBase: 0, dataBase: 0x20000);

        // Recompute checksum in C# from memory to validate the copy + loads/stores.
        uint srcBase = 0x20000;
        uint dstBase = srcBase + 16u * 4u;

        uint checksum = 0;
        for (int i = 0; i < 16; i++)
        {
            uint srcW = s.Memory!.ReadWord(srcBase + (uint)(i * 4));
            uint dstW = s.Memory!.ReadWord(dstBase + (uint)(i * 4));
            Assert.Equal(srcW, dstW);
            checksum ^= dstW;
        }

        Assert.Equal(checksum, (uint)s.Registers.Read(20)); // s4
    }

    [Fact]
    public void Stress_Recursive_Fibonacci_With_Stack_And_ControlFlow()
    {
        var s = NewState(memSize: 0x40000, textBase: 0, dataBase: 0x30000);
        s.Registers.Write(2, 0x20000); // sp

        // fib(12) = 144
        // This pounds: jal/ret, stack, loads/stores, branches, forwarding around returns.
        string code = @"
            .text
            li a0, 12
            jal ra, fib
            mv s0, a0
            ebreak

        fib:
            addi sp, sp, -32
            sd ra, 24(sp)
            sd a0, 16(sp)

            li t0, 1
            ble a0, t0, fib_base

            addi a0, a0, -1
            jal ra, fib
            sd a0, 8(sp)      # save fib(n-1)

            ld a0, 16(sp)
            addi a0, a0, -2
            jal ra, fib       # returns fib(n-2) in a0

            ld t1, 8(sp)      # fib(n-1)
            add a0, a0, t1    # fib(n) = fib(n-2) + fib(n-1)
            j fib_epilogue

        fib_base:
            # a0 is already 0 or 1
            nop

        fib_epilogue:
            ld ra, 24(sp)
            addi sp, sp, 32
            ret
        ";

        RunAsm(s, code, maxCycles: 200000, textBase: 0, dataBase: 0x30000);

        Assert.Equal(144ul, s.Registers.Read(8)); // s0
    }

    [Fact]
    public void Stress_LoadUse_Interlock_And_Forwarding_Around_Loads()
    {
        var s = NewState(memSize: 0x20000, textBase: 0, dataBase: 0x10000);

        // Writes a value, then loads it and immediately uses it in dependent ops.
        // Should trigger load-use hazard handling + forwarding.
        string code = @"
            .text
            li t0, 0x10000
            li t1, 777
            sw t1, 0(t0)

            lw t2, 0(t0)       # t2 = 777
            addi t3, t2, 1     # should be 778 (load-use)
            add t4, t3, t2     # 1555
            addi t5, t4, -555  # 1000
            mv s0, t5

            ebreak
        ";

        RunAsm(s, code, maxCycles: 2000, textBase: 0, dataBase: 0x10000);

        Assert.Equal(1000ul, s.Registers.Read(8)); // s0
    }

    [Fact]
    public void Stress_Long_RandomLike_Mix_NoExceptions_And_SentinelMemory()
    {
        var s = NewState(memSize: 0x80000, textBase: 0, dataBase: 0x60000);
        s.Registers.Write(2, 0x50000); // sp

        // A long mixed program intended to stress:
        // - lots of labels
        // - AUIPC/LUI/ADDI address math (via LA/LI)
        // - branches, jumps, calls/returns
        // - stack saves/loads
        // - stores/loads in a patterned stride
        // - ensures sentinel values appear as expected
        string code = @"
            .data
        buf:
            .word 0 0 0 0 0 0 0 0
            .word 0 0 0 0 0 0 0 0

            .text
            la s0, buf         # base
            li s1, 16          # count
            li s2, 0           # i
            li s3, 0x13579BDF  # seed

        fill:
            # xorshift-ish scramble
            slli t0, s3, 13
            xor  s3, s3, t0
            srli t0, s3, 7
            xor  s3, s3, t0
            slli t0, s3, 17
            xor  s3, s3, t0

            sw s3, 0(s0)
            addi s0, s0, 4
            addi s2, s2, 1
            blt s2, s1, fill

            # reset ptr and compute checksum with a call
            la a0, buf
            li a1, 16
            jal ra, checksum
            mv s4, a0

            # write sentinel 0xC0FFEE to buf[0] and buf[15]
            la t1, buf
            li t2, 0xC0FFEE
            sw t2, 0(t1)
            addi t1, t1, 60
            sw t2, 0(t1)

            ebreak

        checksum:
            # a0=ptr, a1=count
            addi sp, sp, -32
            sd ra, 24(sp)
            sd a0, 16(sp)
            sd a1, 8(sp)

            li t0, 0
            li t1, 0

        chk_loop:
            lw t2, 0(a0)
            xor t0, t0, t2
            addi a0, a0, 4
            addi t1, t1, 1
            blt t1, a1, chk_loop

            mv a0, t0

            ld ra, 24(sp)
            addi sp, sp, 32
            ret
        ";

        RunAsm(s, code, maxCycles: 200000, textBase: 0, dataBase: 0x60000);

        // Validate sentinels were written where expected.
        uint bufBase = 0x60000;
        Assert.Equal(0x00C0FFEEu, s.Memory!.ReadWord(bufBase + 0));
        Assert.Equal(0x00C0FFEEu, s.Memory.ReadWord(bufBase + 60));

        // Also assert checksum register is non-zero and stable type-wise.
        Assert.NotEqual(0ul, s.Registers.Read(20)); // s4
    }
}
