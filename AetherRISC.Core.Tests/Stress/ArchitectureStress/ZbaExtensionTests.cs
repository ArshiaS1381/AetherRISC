using AetherRISC.Core.Architecture.Hardware.ISA.Decoding;
using AetherRISC.Core.Architecture.Hardware.ISA.Encoding;
using AetherRISC.Core.Architecture.Hardware.ISA.Extensions.B.Zba;
using AetherRISC.Core.Architecture.Hardware.ISA.Extensions.B.Zbb;
using AetherRISC.Core.Architecture.Hardware.Memory;
using AetherRISC.Core.Architecture.Simulation;
using AetherRISC.Core.Architecture.Simulation.Runners;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Helpers;
using Xunit;

namespace AetherRISC.Core.Tests.Architecture.Stress;

public class ZbaExtensionTests
{
    [Fact]
    public void Zba_Encoding_Is_Correct_And_Does_Not_Collide_With_Zbb()
    {
        // SH1ADD x7, x5, x6 => funct7=0x10, funct3=2
        var sh1 = new Sh1addInstruction(7, 5, 6);
        uint sh1Enc = InstructionEncoder.Encode(sh1);
        uint sh1Expected =
            (0x10u << 25) | (6u << 20) | (5u << 15) | (2u << 12) | (7u << 7) | 0x33u;
        Assert.Equal(sh1Expected, sh1Enc);

        // SH2ADD x7, x5, x6 => funct7=0x10, funct3=4
        var sh2 = new Sh2addInstruction(7, 5, 6);
        uint sh2Enc = InstructionEncoder.Encode(sh2);
        uint sh2Expected =
            (0x10u << 25) | (6u << 20) | (5u << 15) | (4u << 12) | (7u << 7) | 0x33u;
        Assert.Equal(sh2Expected, sh2Enc);

        // SH3ADD x7, x5, x6 => funct7=0x10, funct3=6
        var sh3 = new Sh3addInstruction(7, 5, 6);
        uint sh3Enc = InstructionEncoder.Encode(sh3);
        uint sh3Expected =
            (0x10u << 25) | (6u << 20) | (5u << 15) | (6u << 12) | (7u << 7) | 0x33u;
        Assert.Equal(sh3Expected, sh3Enc);

        // Must not collide with XNOR (funct7=0x20, funct3=4)
        var xnor = new XnorInstruction(7, 5, 6);
        uint xnorEnc = InstructionEncoder.Encode(xnor);
        Assert.NotEqual(xnorEnc, sh2Enc);
    }

    [Fact]
    public void Zba_Decoding_Works()
    {
        var decoder = new InstructionDecoder();

        uint word =
            (0x10u << 25) | (6u << 20) | (5u << 15) | (4u << 12) | (7u << 7) | 0x33u;
        var inst = decoder.Decode(word);

        Assert.Equal("SH2ADD", inst.Mnemonic);
        Assert.Equal(7, inst.Rd);
        Assert.Equal(5, inst.Rs1);
        Assert.Equal(6, inst.Rs2);
    }

    [Fact]
    public void Zba_Pipeline_Execution_Works()
    {
        var s = new MachineState(SystemConfig.Rv64());
        s.Memory = new SystemBus(0x4000);
        s.Host = new MultiOSHandler { Silent = true };

        string code = @"
            .text
            li t0, 3
            li t1, 100
            sh1add t2, t0, t1
            sh2add t3, t0, t1
            sh3add t4, t0, t1
            ebreak
        ";

        new SourceAssembler(code) { TextBase = 0 }.Assemble(s);
        s.ProgramCounter = 0;

        new PipelinedRunner(s, new NullLogger()).Run(500);

        Assert.Equal(106ul, s.Registers.Read(7));  // t2
        Assert.Equal(112ul, s.Registers.Read(28)); // t3
        Assert.Equal(124ul, s.Registers.Read(29)); // t4
    }
}
