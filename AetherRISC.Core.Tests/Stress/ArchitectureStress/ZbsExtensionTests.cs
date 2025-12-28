using System;
using AetherRISC.Core.Architecture.Hardware.ISA.Decoding;
using AetherRISC.Core.Architecture.Hardware.ISA.Encoding;
using AetherRISC.Core.Architecture.Hardware.Memory;
using AetherRISC.Core.Architecture.Simulation;
using AetherRISC.Core.Architecture.Simulation.Runners;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Helpers;
using Xunit;

namespace AetherRISC.Core.Tests.Architecture.Stress;

public class ZbsExtensionTests
{
    [Fact]
    public void Zbs_Encoding_Is_Correct_For_Key_Ops()
    {
        // BSET x7,x5,x6 => funct7=0x14, funct3=1
        uint bset = (0x14u << 25) | (6u << 20) | (5u << 15) | (1u << 12) | (7u << 7) | 0x33u;
        Assert.Equal(bset, InstructionEncoder.Encode(AetherRISC.Core.Helpers.Inst.Bset(7, 5, 6)));

        // BCLR x7,x5,x6 => funct7=0x24, funct3=1
        uint bclr = (0x24u << 25) | (6u << 20) | (5u << 15) | (1u << 12) | (7u << 7) | 0x33u;
        Assert.Equal(bclr, InstructionEncoder.Encode(AetherRISC.Core.Helpers.Inst.Bclr(7, 5, 6)));

        // BINV x7,x5,x6 => funct7=0x34, funct3=1
        uint binv = (0x34u << 25) | (6u << 20) | (5u << 15) | (1u << 12) | (7u << 7) | 0x33u;
        Assert.Equal(binv, InstructionEncoder.Encode(AetherRISC.Core.Helpers.Inst.Binv(7, 5, 6)));

        // BEXT x7,x5,x6 => funct7=0x24, funct3=5
        uint bext = (0x24u << 25) | (6u << 20) | (5u << 15) | (5u << 12) | (7u << 7) | 0x33u;
        Assert.Equal(bext, InstructionEncoder.Encode(AetherRISC.Core.Helpers.Inst.Bext(7, 5, 6)));
    }

    [Fact]
    public void Zbs_Decoding_Works()
    {
        var decoder = new InstructionDecoder();

        uint word = (0x24u << 25) | (6u << 20) | (5u << 15) | (5u << 12) | (7u << 7) | 0x33u;
        var inst = decoder.Decode(word);

        Assert.Equal("BEXT", inst.Mnemonic);
        Assert.Equal(7, inst.Rd);
        Assert.Equal(5, inst.Rs1);
        Assert.Equal(6, inst.Rs2);
    }

    [Fact]
    public void Zbs_Pipeline_Execution_Works()
    {
        var s = new MachineState(SystemConfig.Rv64());
        s.Memory = new SystemBus(0x8000);
        s.Host = new MultiOSHandler { Silent = true };

        string code = @"
            .text
            li t0, 0
            bseti t0, t0, 3      # 0b1000
            bseti t0, t0, 0      # 0b1001
            bclri t1, t0, 3      # 0b0001
            binvi t2, t1, 2      # 0b0101
            bexti t3, t2, 2      # 1
            bexti t4, t2, 3      # 0

            li t5, 0
            li t6, 5
            bset t5, t5, t6      # set bit5 => 32
            binv t5, t5, t6      # toggle bit5 => 0
            ebreak
        ";

        new SourceAssembler(code) { TextBase = 0 }.Assemble(s);
        s.ProgramCounter = 0;

        new PipelinedRunner(s, new NullLogger()).Run(2000);

        Assert.Equal(0x9ul, s.Registers.Read(5));   // t0
        Assert.Equal(0x1ul, s.Registers.Read(6));   // t1
        Assert.Equal(0x5ul, s.Registers.Read(7));   // t2
        Assert.Equal(0x1ul, s.Registers.Read(28));  // t3
        Assert.Equal(0x0ul, s.Registers.Read(29));  // t4
        Assert.Equal(0x0ul, s.Registers.Read(30));  // t5
    }
}
