using System;
using Xunit;
using Xunit.Abstractions;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Simulation.Runners;
using AetherRISC.Core.Architecture.Simulation;
using AetherRISC.Core.Architecture.Hardware.Memory;
using AetherRISC.Core.Helpers;

namespace AetherRISC.Core.Tests.Architecture.Stress;

public class ZbsComprehensiveStressTests
{
    private readonly ITestOutputHelper _output;
    public ZbsComprehensiveStressTests(ITestOutputHelper output) => _output = output;

    private static ulong MaskForXlen(int xlen)
        => xlen == 32 ? 0xFFFFFFFFul : 0xFFFFFFFFFFFFFFFFul;

    private static int ShamtMask(int xlen) => xlen == 32 ? 31 : 63;

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

    // Zbs reference ops (per spec):
    // bset:  x[rd] = x[rs1] |  (1 << (x[rs2] & (XLEN-1)))
    // bclr:  x[rd] = x[rs1] & ~(1 << (x[rs2] & (XLEN-1)))
    // binv:  x[rd] = x[rs1] ^  (1 << (x[rs2] & (XLEN-1)))
    // bext:  x[rd] = (x[rs1] >> (x[rs2] & (XLEN-1))) & 1
    private static ulong RefBset(ulong rs1, ulong rs2, int xlen)
    {
        ulong m = 1ul << (int)(rs2 & (ulong)ShamtMask(xlen));
        return (rs1 | m) & MaskForXlen(xlen);
    }

    private static ulong RefBclr(ulong rs1, ulong rs2, int xlen)
    {
        ulong m = 1ul << (int)(rs2 & (ulong)ShamtMask(xlen));
        return (rs1 & ~m) & MaskForXlen(xlen);
    }

    private static ulong RefBinv(ulong rs1, ulong rs2, int xlen)
    {
        ulong m = 1ul << (int)(rs2 & (ulong)ShamtMask(xlen));
        return (rs1 ^ m) & MaskForXlen(xlen);
    }

    private static ulong RefBext(ulong rs1, ulong rs2, int xlen)
    {
        int sh = (int)(rs2 & (ulong)ShamtMask(xlen));
        return (rs1 >> sh) & 1ul;
    }

    [Theory]
    [InlineData(64)]
    [InlineData(32)]
    public void Zbs_KnownVectors_RegisterForms(int xlen)
    {
        var s = xlen == 64 ? NewState64() : NewState32();

        // Choose patterns that exercise multiple bits and edge shifts.
        ulong a = xlen == 64 ? 0x0123456789ABCDEFul : 0x89ABCDEFul;
        ulong idx = xlen == 64 ? 63ul : 31ul;

        s.Registers.Write(5, a);   // t0
        s.Registers.Write(6, idx); // t1

        string code = @"
            .text
            bset t2, t0, t1
            bclr t3, t0, t1
            binv t4, t0, t1
            bext t5, t0, t1
            ebreak
        ";

        RunAsm(s, code, maxCycles: 120);

        ulong gotBset = s.Registers.Read(7);
        ulong gotBclr = s.Registers.Read(28);
        ulong gotBinv = s.Registers.Read(29);
        ulong gotBext = s.Registers.Read(30);

        if (xlen == 32)
        {
            gotBset &= 0xFFFFFFFFul;
            gotBclr &= 0xFFFFFFFFul;
            gotBinv &= 0xFFFFFFFFul;
            gotBext &= 0xFFFFFFFFul;
            a &= 0xFFFFFFFFul;
        }

        ulong expBset = RefBset(a, idx, xlen);
        ulong expBclr = RefBclr(a, idx, xlen);
        ulong expBinv = RefBinv(a, idx, xlen);
        ulong expBext = RefBext(a, idx, xlen);

        _output.WriteLine($"XLEN={xlen} a=0x{a:X} idx={idx}");
        _output.WriteLine($"BSET got=0x{gotBset:X} exp=0x{expBset:X}");
        _output.WriteLine($"BCLR got=0x{gotBclr:X} exp=0x{expBclr:X}");
        _output.WriteLine($"BINV got=0x{gotBinv:X} exp=0x{expBinv:X}");
        _output.WriteLine($"BEXT got=0x{gotBext:X} exp=0x{expBext:X}");

        Assert.Equal(expBset, gotBset);
        Assert.Equal(expBclr, gotBclr);
        Assert.Equal(expBinv, gotBinv);
        Assert.Equal(expBext, gotBext);
    }

    [Theory]
    [InlineData(64)]
    [InlineData(32)]
    public void Zbs_ImmediateForms_KnownVectors(int xlen)
    {
        // If your assembler supports the immediate forms: bseti/bclri/binvi/bexti.
        // This test will fail fast if those mnemonics are not supported, which is
        // desirable for coverage.
        var s = xlen == 64 ? NewState64() : NewState32();

        ulong a = xlen == 64 ? 0x0000000000000000ul : 0x00000000ul;
        int imm = xlen == 64 ? 63 : 31;

        s.Registers.Write(5, a); // t0

        string code = $@"
            .text
            bseti t2, t0, {imm}
            bclri t3, t2, {imm}
            binvi t4, t0, {imm}
            bexti t5, t4, {imm}
            ebreak
        ";

        RunAsm(s, code, maxCycles: 140);

        ulong gotBseti = s.Registers.Read(7);
        ulong gotBclri = s.Registers.Read(28);
        ulong gotBinvi = s.Registers.Read(29);
        ulong gotBexti = s.Registers.Read(30);

        if (xlen == 32)
        {
            gotBseti &= 0xFFFFFFFFul;
            gotBclri &= 0xFFFFFFFFul;
            gotBinvi &= 0xFFFFFFFFul;
            gotBexti &= 0xFFFFFFFFul;
        }

        ulong expBseti = RefBset(a, (ulong)imm, xlen);
        ulong expBclri = RefBclr(expBseti, (ulong)imm, xlen);
        ulong expBinvi = RefBinv(a, (ulong)imm, xlen);
        ulong expBexti = RefBext(expBinvi, (ulong)imm, xlen);

        Assert.Equal(expBseti, gotBseti);
        Assert.Equal(expBclri, gotBclri);
        Assert.Equal(expBinvi, gotBinvi);
        Assert.Equal(expBexti, gotBexti);
    }

    [Fact]
    public void Zbs_Random_Stress_RV64()
    {
        var rng = new Random(0x515B515B);
        for (int i = 0; i < 600; i++)
        {
            var s = NewState64();

            ulong a = ((ulong)rng.NextInt64() << 1) ^ (ulong)rng.Next();
            ulong idx = (ulong)rng.Next(0, 256);

            s.Registers.Write(5, a);
            s.Registers.Write(6, idx);

            string code = @"
                .text
                bset t2, t0, t1
                bclr t3, t0, t1
                binv t4, t0, t1
                bext t5, t0, t1
                ebreak
            ";

            RunAsm(s, code, maxCycles: 120);

            Assert.Equal(RefBset(a, idx, 64), s.Registers.Read(7));
            Assert.Equal(RefBclr(a, idx, 64), s.Registers.Read(28));
            Assert.Equal(RefBinv(a, idx, 64), s.Registers.Read(29));
            Assert.Equal(RefBext(a, idx, 64), s.Registers.Read(30));
        }
    }

    [Fact]
    public void Zbs_Random_Stress_RV32()
    {
        var rng = new Random(0x32323232);
        for (int i = 0; i < 600; i++)
        {
            var s = NewState32();

            uint a32 = (uint)rng.NextInt64();
            uint idx32 = (uint)rng.Next(0, 256);

            ulong a = a32;
            ulong idx = idx32;

            s.Registers.Write(5, a);
            s.Registers.Write(6, idx);

            string code = @"
                .text
                bset t2, t0, t1
                bclr t3, t0, t1
                binv t4, t0, t1
                bext t5, t0, t1
                ebreak
            ";

            RunAsm(s, code, maxCycles: 120);

            ulong gotBset = s.Registers.Read(7) & 0xFFFFFFFFul;
            ulong gotBclr = s.Registers.Read(28) & 0xFFFFFFFFul;
            ulong gotBinv = s.Registers.Read(29) & 0xFFFFFFFFul;
            ulong gotBext = s.Registers.Read(30) & 0xFFFFFFFFul;

            ulong expBset = RefBset(a, idx, 32);
            ulong expBclr = RefBclr(a, idx, 32);
            ulong expBinv = RefBinv(a, idx, 32);
            ulong expBext = RefBext(a, idx, 32);

            Assert.Equal(expBset, gotBset);
            Assert.Equal(expBclr, gotBclr);
            Assert.Equal(expBinv, gotBinv);
            Assert.Equal(expBext, gotBext);
        }
    }

    [Fact]
    public void Zbs_Forwarding_RAW_Hazard_RV64()
    {
        // Create a tight sequence where the destination is immediately consumed.
        var s = NewState64();

        // We avoid LI here; use ADDI with small immediates.
        string code = @"
            .text
            addi t0, zero, 1
            addi t1, zero, 63
            bset t2, t0, t1
            bext t3, t2, t1
            ebreak
        ";

        RunAsm(s, code, maxCycles: 120);

        ulong t0 = s.Registers.Read(5);
        ulong t1 = s.Registers.Read(6);
        ulong t2 = s.Registers.Read(7);
        ulong t3 = s.Registers.Read(28);

        ulong expT2 = RefBset(t0, t1, 64);
        ulong expT3 = RefBext(expT2, t1, 64);

        Assert.Equal(expT2, t2);
        Assert.Equal(expT3, t3);
        Assert.Equal(1ul, t3);
    }
}
