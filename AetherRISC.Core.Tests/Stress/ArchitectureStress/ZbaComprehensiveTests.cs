using Xunit;
using Xunit.Abstractions;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Simulation.Runners;
using AetherRISC.Core.Architecture.Simulation;
using AetherRISC.Core.Architecture.Hardware.Memory;
using AetherRISC.Core.Architecture.Hardware.ISA.Encoding;
using AetherRISC.Core.Architecture.Hardware.ISA.Decoding;
using AetherRISC.Core.Helpers;

namespace AetherRISC.Core.Tests.Architecture.Stress;

/// <summary>
/// Comprehensive tests for Zba (Address Generation) extension
/// Instructions: ADD.UW, SH1ADD, SH2ADD, SH3ADD, SH1ADD.UW, SH2ADD.UW, SH3ADD.UW, SLLI.UW
/// </summary>
public class ZbaComprehensiveTests
{
    private readonly ITestOutputHelper _output;
    public ZbaComprehensiveTests(ITestOutputHelper output) => _output = output;

    #region SH1ADD Tests (Shift-left 1 and Add)

    [Theory]
    [InlineData(0x00000001ul, 0x00000002ul, 0x00000004ul)] // 1<<1 + 2 = 4
    [InlineData(0x00000010ul, 0x00000005ul, 0x00000025ul)] // 16<<1 + 5 = 37
    [InlineData(0x7FFFFFFFul, 0x00000001ul, 0x00000000FFFFFFFFul)] // Near overflow RV64
    [InlineData(0x00000000ul, 0x00000000ul, 0x00000000ul)] // Zero case
    [InlineData(0xFFFFFFFFul, 0x00000001ul, 0x00000001FFFFFFFFul)] // Max 32-bit << 1 + 1
    public void Sh1add_RV64_Computes_Correctly(ulong rs1, ulong rs2, ulong expected)
    {
        var s = new MachineState(SystemConfig.Rv64());
        s.Memory = new SystemBus(0x8000);
        s.Host = new MultiOSHandler { Silent = true };

        s.Registers.Write(5, rs1); // t0
        s.Registers.Write(6, rs2); // t1

        string code = @"
            .text
            sh1add t2, t0, t1
            ebreak
        ";

        new SourceAssembler(code) { TextBase = 0 }.Assemble(s);
        s.ProgramCounter = 0;
        new PipelinedRunner(s, new NullLogger()).Run(20);

        ulong result = s.Registers.Read(7);
        _output.WriteLine($"SH1ADD: 0x{rs1:X} << 1 + 0x{rs2:X} = 0x{result:X} (expected 0x{expected:X})");
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Sh1add_RV32_Wraps_At_32Bits()
    {
        var s = new MachineState(SystemConfig.Rv32());
        s.Memory = new SystemBus(0x8000);
        s.Host = new MultiOSHandler { Silent = true };

        s.Registers.Write(5, 0x80000000ul); // t0 - will overflow when shifted
        s.Registers.Write(6, 0x00000001ul); // t1

        string code = @"
            .text
            sh1add t2, t0, t1
            ebreak
        ";

        new SourceAssembler(code) { TextBase = 0 }.Assemble(s);
        s.ProgramCounter = 0;
        new PipelinedRunner(s, new NullLogger()).Run(20);

        ulong result = s.Registers.Read(7);
        // 0x80000000 << 1 = 0x100000000, truncated to 32 bits = 0, + 1 = 1
        Assert.Equal(0x00000001ul, result & 0xFFFFFFFF);
    }

    #endregion

    #region SH2ADD Tests (Shift-left 2 and Add)

    [Theory]
    [InlineData(0x00000001ul, 0x00000000ul, 0x00000004ul)] // 1<<2 + 0 = 4
    [InlineData(0x00000004ul, 0x00000010ul, 0x00000020ul)] // 4<<2 + 16 = 32
    [InlineData(0x10000000ul, 0x00000000ul, 0x40000000ul)] // Large shift
    public void Sh2add_RV64_Computes_Correctly(ulong rs1, ulong rs2, ulong expected)
    {
        var s = new MachineState(SystemConfig.Rv64());
        s.Memory = new SystemBus(0x8000);
        s.Host = new MultiOSHandler { Silent = true };

        s.Registers.Write(5, rs1);
        s.Registers.Write(6, rs2);

        string code = @"
            .text
            sh2add t2, t0, t1
            ebreak
        ";

        new SourceAssembler(code) { TextBase = 0 }.Assemble(s);
        s.ProgramCounter = 0;
        new PipelinedRunner(s, new NullLogger()).Run(20);

        Assert.Equal(expected, s.Registers.Read(7));
    }

    #endregion

    #region SH3ADD Tests (Shift-left 3 and Add)

    [Theory]
    [InlineData(0x00000001ul, 0x00000000ul, 0x00000008ul)] // 1<<3 = 8
    [InlineData(0x00000002ul, 0x00000008ul, 0x00000018ul)] // 2<<3 + 8 = 24
    [InlineData(0x08000000ul, 0x00000000ul, 0x40000000ul)] // Large value
    public void Sh3add_RV64_Computes_Correctly(ulong rs1, ulong rs2, ulong expected)
    {
        var s = new MachineState(SystemConfig.Rv64());
        s.Memory = new SystemBus(0x8000);
        s.Host = new MultiOSHandler { Silent = true };

        s.Registers.Write(5, rs1);
        s.Registers.Write(6, rs2);

        string code = @"
            .text
            sh3add t2, t0, t1
            ebreak
        ";

        new SourceAssembler(code) { TextBase = 0 }.Assemble(s);
        s.ProgramCounter = 0;
        new PipelinedRunner(s, new NullLogger()).Run(20);

        Assert.Equal(expected, s.Registers.Read(7));
    }

    #endregion

    #region ADD.UW Tests (RV64 only - Add Unsigned Word)

    [Theory]
    [InlineData(0xFFFFFFFF80000000ul, 0x0000000000000001ul, 0xFFFFFFFF80000001ul)] // Zero-extends rs1
    [InlineData(0xFFFFFFFFFFFFFFFFul, 0x0000000000000001ul, 0x0000000000000000ul)] // -1 as 32-bit = 0xFFFFFFFF
    [InlineData(0x0000000012345678ul, 0x0000000000000000ul, 0x0000000012345678ul)] // No change needed
    public void AddUw_RV64_ZeroExtends_Rs1(ulong rs1, ulong rs2, ulong expected)
    {
        var s = new MachineState(SystemConfig.Rv64());
        s.Memory = new SystemBus(0x8000);
        s.Host = new MultiOSHandler { Silent = true };

        s.Registers.Write(5, rs1);
        s.Registers.Write(6, rs2);

        string code = @"
            .text
            add.uw t2, t0, t1
            ebreak
        ";

        new SourceAssembler(code) { TextBase = 0 }.Assemble(s);
        s.ProgramCounter = 0;
        new PipelinedRunner(s, new NullLogger()).Run(20);

        ulong result = s.Registers.Read(7);
        _output.WriteLine($"ADD.UW: zext(0x{rs1:X}) + 0x{rs2:X} = 0x{result:X}");
        Assert.Equal(expected, result);
    }

    #endregion

    #region SH*ADD.UW Tests (RV64 only)

    [Theory]
    [InlineData(0xFFFFFFFF80000000ul, 0x00000000ul, 0x0000000100000000ul)] // zext(0x80000000) << 1
    public void Sh1addUw_RV64_ZeroExtends_And_Shifts(ulong rs1, ulong rs2, ulong expected)
    {
        var s = new MachineState(SystemConfig.Rv64());
        s.Memory = new SystemBus(0x8000);
        s.Host = new MultiOSHandler { Silent = true };

        s.Registers.Write(5, rs1);
        s.Registers.Write(6, rs2);

        string code = @"
            .text
            sh1add.uw t2, t0, t1
            ebreak
        ";

        new SourceAssembler(code) { TextBase = 0 }.Assemble(s);
        s.ProgramCounter = 0;
        new PipelinedRunner(s, new NullLogger()).Run(20);

        Assert.Equal(expected, s.Registers.Read(7));
    }

    [Theory]
    [InlineData(0xFFFFFFFF40000000ul, 0x00000000ul, 0x0000000100000000ul)] // zext(0x40000000) << 2
    public void Sh2addUw_RV64_ZeroExtends_And_Shifts(ulong rs1, ulong rs2, ulong expected)
    {
        var s = new MachineState(SystemConfig.Rv64());
        s.Memory = new SystemBus(0x8000);
        s.Host = new MultiOSHandler { Silent = true };

        s.Registers.Write(5, rs1);
        s.Registers.Write(6, rs2);

        string code = @"
            .text
            sh2add.uw t2, t0, t1
            ebreak
        ";

        new SourceAssembler(code) { TextBase = 0 }.Assemble(s);
        s.ProgramCounter = 0;
        new PipelinedRunner(s, new NullLogger()).Run(20);

        Assert.Equal(expected, s.Registers.Read(7));
    }

    [Theory]
    [InlineData(0xFFFFFFFF20000000ul, 0x00000000ul, 0x0000000100000000ul)] // zext(0x20000000) << 3
    public void Sh3addUw_RV64_ZeroExtends_And_Shifts(ulong rs1, ulong rs2, ulong expected)
    {
        var s = new MachineState(SystemConfig.Rv64());
        s.Memory = new SystemBus(0x8000);
        s.Host = new MultiOSHandler { Silent = true };

        s.Registers.Write(5, rs1);
        s.Registers.Write(6, rs2);

        string code = @"
            .text
            sh3add.uw t2, t0, t1
            ebreak
        ";

        new SourceAssembler(code) { TextBase = 0 }.Assemble(s);
        s.ProgramCounter = 0;
        new PipelinedRunner(s, new NullLogger()).Run(20);

        Assert.Equal(expected, s.Registers.Read(7));
    }

    #endregion

    #region SLLI.UW Tests (RV64 only)

    [Theory]
    [InlineData(0xFFFFFFFFFFFFFFFFul, 4, 0x0000000FFFFFFFF0ul)] // zext then shift
    [InlineData(0x0000000080000000ul, 1, 0x0000000100000000ul)] // Shift into upper 32
    [InlineData(0xFFFFFFFF00000001ul, 31, 0x0000000080000000ul)] // Max 32-bit shift
    public void SlliUw_RV64_ZeroExtends_Then_Shifts(ulong rs1, int shamt, ulong expected)
    {
        var s = new MachineState(SystemConfig.Rv64());
        s.Memory = new SystemBus(0x8000);
        s.Host = new MultiOSHandler { Silent = true };

        s.Registers.Write(5, rs1);

        string code = $@"
            .text
            slli.uw t2, t0, {shamt}
            ebreak
        ";

        new SourceAssembler(code) { TextBase = 0 }.Assemble(s);
        s.ProgramCounter = 0;
        new PipelinedRunner(s, new NullLogger()).Run(20);

        ulong result = s.Registers.Read(7);
        _output.WriteLine($"SLLI.UW: zext(0x{rs1:X}) << {shamt} = 0x{result:X}");
        Assert.Equal(expected, result);
    }

    #endregion

    #region Encoding/Decoding Tests

    [Fact]
    public void Zba_Instructions_Encode_Correctly()
    {
        var decoder = new InstructionDecoder();

        // SH1ADD rd, rs1, rs2: funct7=0x10, funct3=2, opcode=0x33
        uint sh1add = InstructionEncoder.Encode(Inst.Sh1add(7, 5, 6));
        Assert.Equal(0x33u, sh1add & 0x7F); // opcode
        Assert.Equal(2u, (sh1add >> 12) & 0x7); // funct3
        Assert.Equal(0x10u, (sh1add >> 25) & 0x7F); // funct7

        // SH2ADD: funct3=4
        uint sh2add = InstructionEncoder.Encode(Inst.Sh2add(7, 5, 6));
        Assert.Equal(4u, (sh2add >> 12) & 0x7);

        // SH3ADD: funct3=6
        uint sh3add = InstructionEncoder.Encode(Inst.Sh3add(7, 5, 6));
        Assert.Equal(6u, (sh3add >> 12) & 0x7);
    }

    [Fact]
    public void Zba_Instructions_Decode_Correctly()
    {
        var decoder = new InstructionDecoder();

        // Test SH1ADD decoding
        uint sh1add = (0x10u << 25) | (6u << 20) | (5u << 15) | (2u << 12) | (7u << 7) | 0x33u;
        var inst = decoder.Decode(sh1add);
        Assert.Equal("SH1ADD", inst.Mnemonic);
        Assert.Equal(7, inst.Rd);
        Assert.Equal(5, inst.Rs1);
        Assert.Equal(6, inst.Rs2);
    }

    #endregion

    #region Address Calculation Patterns

    [Fact]
    public void Zba_Array_Indexing_Pattern_16bit()
    {
        // Pattern: base + index * 2 (16-bit array element access)
        var s = new MachineState(SystemConfig.Rv64());
        s.Memory = new SystemBus(0x8000);
        s.Host = new MultiOSHandler { Silent = true };

        ulong baseAddr = 0x1000;
        ulong index = 5;

        s.Registers.Write(5, index);      // t0 = index
        s.Registers.Write(6, baseAddr);   // t1 = base

        string code = @"
            .text
            sh1add t2, t0, t1
            ebreak
        ";

        new SourceAssembler(code) { TextBase = 0 }.Assemble(s);
        s.ProgramCounter = 0;
        new PipelinedRunner(s, new NullLogger()).Run(20);

        Assert.Equal(baseAddr + index * 2, s.Registers.Read(7));
    }

    [Fact]
    public void Zba_Array_Indexing_Pattern_32bit()
    {
        // Pattern: base + index * 4 (32-bit array element access)
        var s = new MachineState(SystemConfig.Rv64());
        s.Memory = new SystemBus(0x8000);
        s.Host = new MultiOSHandler { Silent = true };

        ulong baseAddr = 0x2000;
        ulong index = 10;

        s.Registers.Write(5, index);
        s.Registers.Write(6, baseAddr);

        string code = @"
            .text
            sh2add t2, t0, t1
            ebreak
        ";

        new SourceAssembler(code) { TextBase = 0 }.Assemble(s);
        s.ProgramCounter = 0;
        new PipelinedRunner(s, new NullLogger()).Run(20);

        Assert.Equal(baseAddr + index * 4, s.Registers.Read(7));
    }

    [Fact]
    public void Zba_Array_Indexing_Pattern_64bit()
    {
        // Pattern: base + index * 8 (64-bit array element access)
        var s = new MachineState(SystemConfig.Rv64());
        s.Memory = new SystemBus(0x8000);
        s.Host = new MultiOSHandler { Silent = true };

        ulong baseAddr = 0x3000;
        ulong index = 7;

        s.Registers.Write(5, index);
        s.Registers.Write(6, baseAddr);

        string code = @"
            .text
            sh3add t2, t0, t1
            ebreak
        ";

        new SourceAssembler(code) { TextBase = 0 }.Assemble(s);
        s.ProgramCounter = 0;
        new PipelinedRunner(s, new NullLogger()).Run(20);

        Assert.Equal(baseAddr + index * 8, s.Registers.Read(7));
    }

    #endregion
}



