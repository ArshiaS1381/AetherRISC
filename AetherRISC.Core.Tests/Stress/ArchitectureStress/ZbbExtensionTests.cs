using AetherRISC.Core.Architecture.Hardware.ISA.Decoding;
using AetherRISC.Core.Architecture.Hardware.ISA.Encoding;
using AetherRISC.Core.Architecture.Hardware.ISA.Extensions.B.Zbb;
using AetherRISC.Core.Architecture.Hardware.Memory;
using AetherRISC.Core.Architecture.Simulation;
using AetherRISC.Core.Architecture.Simulation.Runners;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Helpers;
using Xunit;

namespace AetherRISC.Core.Tests.Architecture.Stress;

public class ZbbExtensionTests
{
    [Fact]
    public void Zbb_Encoding_Is_Correct_For_Core_Ops()
    {
        // ANDN x7, x5, x6
        var andn = new AndnInstruction(7, 5, 6);
        uint andnEnc = InstructionEncoder.Encode(andn);
        uint andnExpected = (0x20u << 25) | (6u << 20) | (5u << 15) | (7u << 12) | (7u << 7) | 0x33u;
        Assert.Equal(andnExpected, andnEnc);

        // ORN x7, x5, x6
        var orn = new OrnInstruction(7, 5, 6);
        uint ornEnc = InstructionEncoder.Encode(orn);
        uint ornExpected = (0x20u << 25) | (6u << 20) | (5u << 15) | (6u << 12) | (7u << 7) | 0x33u;
        Assert.Equal(ornExpected, ornEnc);

        // XNOR x7, x5, x6
        var xnor = new XnorInstruction(7, 5, 6);
        uint xnorEnc = InstructionEncoder.Encode(xnor);
        uint xnorExpected = (0x20u << 25) | (6u << 20) | (5u << 15) | (4u << 12) | (7u << 7) | 0x33u;
        Assert.Equal(xnorExpected, xnorEnc);

        // ROL x7, x5, x6
        var rol = new RolInstruction(7, 5, 6);
        uint rolEnc = InstructionEncoder.Encode(rol);
        uint rolExpected = (0x30u << 25) | (6u << 20) | (5u << 15) | (1u << 12) | (7u << 7) | 0x33u;
        Assert.Equal(rolExpected, rolEnc);

        // ROR x7, x5, x6
        var ror = new RorInstruction(7, 5, 6);
        uint rorEnc = InstructionEncoder.Encode(ror);
        uint rorExpected = (0x30u << 25) | (6u << 20) | (5u << 15) | (5u << 12) | (7u << 7) | 0x33u;
        Assert.Equal(rorExpected, rorEnc);

        // RORI x5, x5, 1  (funct6=0x30, shamt=1)
        var rori = new RoriInstruction(5, 5, 1);
        uint roriEnc = InstructionEncoder.Encode(rori);
        uint imm = 0xC00u | 1u; // 0x30<<6 | 1
        uint roriExpected = (imm << 20) | (5u << 15) | (5u << 12) | (5u << 7) | 0x13u;
        Assert.Equal(roriExpected, roriEnc);
    }

    [Fact]
    public void Zbb_Decoding_Picks_Extension_Instructions_Over_Base()
    {
        var decoder = new InstructionDecoder();

        uint ornWord = (0x20u << 25) | (6u << 20) | (5u << 15) | (6u << 12) | (7u << 7) | 0x33u;
        var inst = decoder.Decode(ornWord);

        Assert.Equal("ORN", inst.Mnemonic);
        Assert.Equal(7, inst.Rd);
        Assert.Equal(5, inst.Rs1);
        Assert.Equal(6, inst.Rs2);
    }

    [Fact]
    public void Zbb_Pipeline_Execution_Works()
    {
        var s = new MachineState(SystemConfig.Rv64());
        s.Memory = new SystemBus(0x4000);
        s.Host = new MultiOSHandler { Silent = true };

        string code = @"
            .text
            li t0, 0x0F0F0F0F
            li t1, 0x00FF00FF

            andn t2, t0, t1
            orn  t3, t0, t1
            xnor t4, t0, t1

            li t5, 1
            rol  t6, t0, t5
            rori s0, t0, 8

            ebreak
        ";

        new SourceAssembler(code) { TextBase = 0 }.Assemble(s);
        s.ProgramCounter = 0;

        new PipelinedRunner(s, new NullLogger()).Run(500);

        ulong t0 = 0x000000000F0F0F0Ful;
        ulong t1 = 0x0000000000FF00FFul;

        ulong andnExpected = t0 & ~t1;
        ulong ornExpected = t0 | ~t1;
        ulong xnorExpected = ~(t0 ^ t1);

        ulong rolExpected = (t0 << 1) | (t0 >> 63);
        ulong roriExpected = (t0 >> 8) | (t0 << (64 - 8));

        Assert.Equal(andnExpected, s.Registers.Read(7));  // t2
        Assert.Equal(ornExpected, s.Registers.Read(28));  // t3
        Assert.Equal(xnorExpected, s.Registers.Read(29)); // t4
        Assert.Equal(rolExpected, s.Registers.Read(31));  // t6
        Assert.Equal(roriExpected, s.Registers.Read(8));  // s0
    }
}
