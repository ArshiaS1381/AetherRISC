using System;
using System.Numerics;
using Xunit;
using Xunit.Abstractions;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Simulation.Runners;
using AetherRISC.Core.Architecture.Simulation;
using AetherRISC.Core.Architecture.Hardware.Memory;
using AetherRISC.Core.Helpers;

namespace AetherRISC.Core.Tests.Architecture.Stress;

public class ZbcComprehensiveStressTests
{
    private readonly ITestOutputHelper _output;
    public ZbcComprehensiveStressTests(ITestOutputHelper output) => _output = output;

    private static (ulong clmul, ulong clmulh, ulong clmulr) RefZbc(
        ulong aRaw,
        ulong bRaw,
        int xlen
    )
    {
        if (xlen != 32 && xlen != 64)
            throw new ArgumentOutOfRangeException(nameof(xlen));

        int bits = xlen;

        BigInteger a = new BigInteger(aRaw);
        BigInteger b = new BigInteger(bRaw);

        BigInteger maskX = (BigInteger.One << bits) - 1;
        a &= maskX;
        b &= maskX;

        BigInteger p = BigInteger.Zero;
        for (int i = 0; i < bits; i++)
        {
            if (((b >> i) & BigInteger.One) != BigInteger.Zero)
                p ^= (a << i);
        }

        BigInteger maskOut = (BigInteger.One << bits) - 1;

        BigInteger lo = p & maskOut;
        BigInteger hi = (p >> bits) & maskOut;

        BigInteger r;
        if (bits == 64)
        {
            // CLMULR returns bits [127:64] shifted right by 1, i.e. p[126:63]
            // Equivalent: (hi << 1) | (lo >> 63) for 64-bit halves.
            r = ((hi << 1) | (lo >> 63)) & maskOut;
        }
        else
        {
            // XLEN=32: CLMULR returns p[62:31] which is (p >> 31) & 0xFFFFFFFF.
            r = (p >> 31) & maskOut;
        }

        return ((ulong)lo, (ulong)hi, (ulong)r);
    }

    private static MachineState NewState32()
    {
        var s = new MachineState(SystemConfig.Rv32());
        s.Memory = new SystemBus(0x8000);
        s.Host = new MultiOSHandler { Silent = true };
        return s;
    }

    private static MachineState NewState64()
    {
        var s = new MachineState(SystemConfig.Rv64());
        s.Memory = new SystemBus(0x8000);
        s.Host = new MultiOSHandler { Silent = true };
        return s;
    }

    private static void RunAsm(MachineState s, string code, int maxCycles = 80)
    {
        new SourceAssembler(code) { TextBase = 0 }.Assemble(s);
        s.ProgramCounter = 0;
        new PipelinedRunner(s, new NullLogger()).Run(maxCycles);
    }

    [Theory]
    [InlineData(64, 0ul, 0ul)]
    [InlineData(64, 1ul, 1ul)]
    [InlineData(64, 0x12345678ul, 0x1ABCDEF0ul)]
    [InlineData(64, 0x8000000000000000ul, 0x8000000000000000ul)]
    [InlineData(64, 0xFFFFFFFFFFFFFFFFul, 0xFFFFFFFFFFFFFFFFul)]
    [InlineData(32, 0u, 0u)]
    [InlineData(32, 1u, 1u)]
    [InlineData(32, 0x12345678u, 0x1ABCDEF0u)]
    [InlineData(32, 0x80000000u, 0x80000000u)]
    [InlineData(32, 0xFFFFFFFFu, 0xFFFFFFFFu)]
    public void Zbc_Pipeline_Matches_Reference_KnownVectors(int xlen, ulong a, ulong b)
    {
        var s = xlen == 64 ? NewState64() : NewState32();

        s.Registers.Write(5, a); // t0
        s.Registers.Write(6, b); // t1

        string code = @"
            .text
            clmul  t2, t0, t1
            clmulh t3, t0, t1
            clmulr t4, t0, t1
            ebreak
        ";

        RunAsm(s, code, maxCycles: 120);

        var (expLo, expHi, expR) = RefZbc(a, b, xlen);

        ulong gotLo = s.Registers.Read(7);  // t2
        ulong gotHi = s.Registers.Read(28); // t3
        ulong gotR = s.Registers.Read(29);  // t4

        if (xlen == 32)
        {
            gotLo &= 0xFFFFFFFFul;
            gotHi &= 0xFFFFFFFFul;
            gotR &= 0xFFFFFFFFul;
        }

        _output.WriteLine($"XLEN={xlen}");
        _output.WriteLine($"a=0x{a:X16} b=0x{b:X16}");
        _output.WriteLine($"CLMUL  got=0x{gotLo:X16} exp=0x{expLo:X16}");
        _output.WriteLine($"CLMULH got=0x{gotHi:X16} exp=0x{expHi:X16}");
        _output.WriteLine($"CLMULR got=0x{gotR:X16} exp=0x{expR:X16}");

        Assert.Equal(expLo, gotLo);
        Assert.Equal(expHi, gotHi);
        Assert.Equal(expR, gotR);
    }

    [Fact]
    public void Zbc_Forwarding_RAW_Hazard_RV64()
    {
        // Tight RAW hazards to stress forwarding:
        // addi writes t0; clmul immediately uses it.
        var s = NewState64();

        string code = @"
            .text
            addi t0, zero, 0x7FF
            addi t1, zero, 0x3A5
            clmul t2, t0, t1
            ebreak
        ";

        RunAsm(s, code, maxCycles: 80);

        ulong a = s.Registers.Read(5);
        ulong b = s.Registers.Read(6);
        ulong got = s.Registers.Read(7);

        var (exp, _, _) = RefZbc(a, b, 64);

        Assert.Equal(exp, got);
    }

    [Fact]
    public void Zbc_Random_Stress_RV64()
    {
        var rng = new Random(0xC0FFEE);
        for (int i = 0; i < 400; i++)
        {
            var s = NewState64();

            ulong a = ((ulong)rng.NextInt64() << 1) ^ (ulong)rng.Next();
            ulong b = ((ulong)rng.NextInt64() << 1) ^ (ulong)rng.Next();

            s.Registers.Write(5, a);
            s.Registers.Write(6, b);

            string code = @"
                .text
                clmul  t2, t0, t1
                clmulh t3, t0, t1
                clmulr t4, t0, t1
                ebreak
            ";

            RunAsm(s, code, maxCycles: 120);

            var (expLo, expHi, expR) = RefZbc(a, b, 64);

            Assert.Equal(expLo, s.Registers.Read(7));
            Assert.Equal(expHi, s.Registers.Read(28));
            Assert.Equal(expR, s.Registers.Read(29));
        }
    }

    [Fact]
    public void Zbc_Random_Stress_RV32()
    {
        var rng = new Random(0xBADC0DE);
        for (int i = 0; i < 400; i++)
        {
            var s = NewState32();

            uint a32 = (uint)rng.NextInt64();
            uint b32 = (uint)rng.NextInt64();

            s.Registers.Write(5, a32);
            s.Registers.Write(6, b32);

            string code = @"
                .text
                clmul  t2, t0, t1
                clmulh t3, t0, t1
                clmulr t4, t0, t1
                ebreak
            ";

            RunAsm(s, code, maxCycles: 120);

            var (expLo, expHi, expR) = RefZbc(a32, b32, 32);

            ulong gotLo = s.Registers.Read(7) & 0xFFFFFFFFul;
            ulong gotHi = s.Registers.Read(28) & 0xFFFFFFFFul;
            ulong gotR = s.Registers.Read(29) & 0xFFFFFFFFul;

            Assert.Equal(expLo, gotLo);
            Assert.Equal(expHi, gotHi);
            Assert.Equal(expR, gotR);
        }
    }
}
