using System;
using System.Numerics;
using AetherRISC.Core.Architecture.Hardware.ISA.Decoding;
using AetherRISC.Core.Architecture.Hardware.ISA.Encoding;
using AetherRISC.Core.Architecture.Hardware.Memory;
using AetherRISC.Core.Architecture.Hardware.ISA.Extensions.B.Zbc;
using AetherRISC.Core.Architecture.Simulation;
using AetherRISC.Core.Architecture.Simulation.Runners;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Helpers;
using Xunit;

namespace AetherRISC.Core.Tests.Architecture.Stress;

public class ZbcExtensionTests
{
    private static (ulong lo, ulong hi) RefClmul128(ulong a, ulong b)
    {
        BigInteger A = new BigInteger(a);
        BigInteger B = new BigInteger(b);
        BigInteger P = BigInteger.Zero;

        for (int i = 0; i < 64; i++)
        {
            if (((b >> i) & 1ul) != 0)
                P ^= (A << i);
        }

        BigInteger mask64 = (BigInteger.One << 64) - 1;
        ulong lo = (ulong)(P & mask64);
        ulong hi = (ulong)((P >> 64) & mask64);
        return (lo, hi);
    }

    [Fact]
    public void Zbc_Encoding_Is_Correct()
    {
        // CLMUL x7,x5,x6 => funct7=0x05, funct3=1
        uint clmul = (0x05u << 25) | (6u << 20) | (5u << 15) | (1u << 12) | (7u << 7) | 0x33u;
        Assert.Equal(clmul, InstructionEncoder.Encode(Inst.Clmul(7, 5, 6)));

        // CLMULR => funct3=2
        uint clmulr = (0x05u << 25) | (6u << 20) | (5u << 15) | (2u << 12) | (7u << 7) | 0x33u;
        Assert.Equal(clmulr, InstructionEncoder.Encode(Inst.Clmulr(7, 5, 6)));

        // CLMULH => funct3=3
        uint clmulh = (0x05u << 25) | (6u << 20) | (5u << 15) | (3u << 12) | (7u << 7) | 0x33u;
        Assert.Equal(clmulh, InstructionEncoder.Encode(Inst.Clmulh(7, 5, 6)));
    }

    [Fact]
    public void Zbc_Decoding_Works()
    {
        var decoder = new InstructionDecoder();

        uint word = (0x05u << 25) | (6u << 20) | (5u << 15) | (3u << 12) | (7u << 7) | 0x33u;
        var inst = decoder.Decode(word);

        Assert.Equal("CLMULH", inst.Mnemonic);
        Assert.Equal(7, inst.Rd);
        Assert.Equal(5, inst.Rs1);
        Assert.Equal(6, inst.Rs2);
    }

    [Fact]
    public void Zbc_Pipeline_Execution_Matches_Reference()
    {
        var s = new MachineState(SystemConfig.Rv64());
        s.Memory = new SystemBus(0x8000);
        s.Host = new MultiOSHandler { Silent = true };

        // Use values that DON'T have bit 31 set to avoid LUI sign-extension issues
        string code = @"
            .text
            li t0, 0x12345678
            li t1, 0x1ABCDEF0
            clmul  t2, t0, t1
            clmulh t3, t0, t1
            clmulr t4, t0, t1
            ebreak
        ";

        new SourceAssembler(code) { TextBase = 0 }.Assemble(s);
        s.ProgramCounter = 0;

        new PipelinedRunner(s, new NullLogger()).Run(2000);

        // These values have bit 31 = 0, so no sign-extension weirdness
        ulong a = 0x12345678ul;
        ulong b = 0x1ABCDEF0ul;

        var (lo, hi) = RefClmul128(a, b);
        ulong r = (hi << 1) | (lo >> 63);

        Assert.Equal(lo, s.Registers.Read(7));   // t2
        Assert.Equal(hi, s.Registers.Read(28));  // t3
        Assert.Equal(r,  s.Registers.Read(29));  // t4
    }

    [Fact]
    public void Zbc_With_SignExtended_Values_Matches_Reference()
    {
        var s = new MachineState(SystemConfig.Rv64());
        s.Memory = new SystemBus(0x8000);
        s.Host = new MultiOSHandler { Silent = true };

        // Test with values where bit 31 IS set - LUI will sign-extend these
        string code = @"
            .text
            li t0, 0x12345678
            li t1, 0x9ABCDEF0
            clmul  t2, t0, t1
            clmulh t3, t0, t1
            clmulr t4, t0, t1
            ebreak
        ";

        new SourceAssembler(code) { TextBase = 0 }.Assemble(s);
        s.ProgramCounter = 0;

        new PipelinedRunner(s, new NullLogger()).Run(2000);

        // IMPORTANT: 0x9ABCDEF0 gets sign-extended to 0xFFFFFFFF9ABCDEF0 by LUI on RV64!
        ulong a = 0x0000000012345678ul;
        ulong b = 0xFFFFFFFF9ABCDEF0ul;  // Sign-extended!

        var (lo, hi) = RefClmul128(a, b);
        ulong r = (hi << 1) | (lo >> 63);

        Assert.Equal(lo, s.Registers.Read(7));   // t2
        Assert.Equal(hi, s.Registers.Read(28));  // t3
        Assert.Equal(r,  s.Registers.Read(29));  // t4
    }
}
