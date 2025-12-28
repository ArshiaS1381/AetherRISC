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

public class ZbbComprehensiveTests
{
    private readonly ITestOutputHelper _output;
    public ZbbComprehensiveTests(ITestOutputHelper output) => _output = output;

    #region ANDN Tests
    [Theory]
    [InlineData(0xFFFFFFFFul, 0x0F0F0F0Ful, 0xF0F0F0F0ul)]
    [InlineData(0xAAAAAAAAul, 0x55555555ul, 0xAAAAAAAAul)]
    [InlineData(0xFFFFFFFFul, 0xFFFFFFFFul, 0x00000000ul)]
    [InlineData(0x12345678ul, 0x00000000ul, 0x12345678ul)]
    public void Andn_RV64_Computes_Correctly(ulong rs1, ulong rs2, ulong expected)
    {
        var s = new MachineState(SystemConfig.Rv64());
        s.Memory = new SystemBus(0x8000);
        s.Host = new MultiOSHandler { Silent = true };
        s.Registers.Write(5, rs1);
        s.Registers.Write(6, rs2);
        string code = ".text\nandn t2, t0, t1\nebreak";
        new SourceAssembler(code) { TextBase = 0 }.Assemble(s);
        s.ProgramCounter = 0;
        new PipelinedRunner(s, new NullLogger()).Run(20);
        Assert.Equal(expected, s.Registers.Read(7));
    }
    #endregion

    #region ORN Tests
    [Theory]
    [InlineData(0x00000000ul, 0xFFFFFFFFul, 0xFFFFFFFF00000000ul)]
    [InlineData(0x00000000ul, 0x0F0F0F0Ful, 0xFFFFFFFFF0F0F0F0ul)]
    [InlineData(0xFFFFFFFFul, 0x00000000ul, 0xFFFFFFFFFFFFFFFFul)]
    public void Orn_RV64_Computes_Correctly(ulong rs1, ulong rs2, ulong expected)
    {
        var s = new MachineState(SystemConfig.Rv64());
        s.Memory = new SystemBus(0x8000);
        s.Host = new MultiOSHandler { Silent = true };
        s.Registers.Write(5, rs1);
        s.Registers.Write(6, rs2);
        string code = ".text\norn t2, t0, t1\nebreak";
        new SourceAssembler(code) { TextBase = 0 }.Assemble(s);
        s.ProgramCounter = 0;
        new PipelinedRunner(s, new NullLogger()).Run(20);
        Assert.Equal(expected, s.Registers.Read(7));
    }
    #endregion

    #region XNOR Tests
    [Theory]
    [InlineData(0xAAAAAAAAul, 0xAAAAAAAAul, 0xFFFFFFFFFFFFFFFFul)]
    [InlineData(0xAAAAAAAAul, 0x55555555ul, 0xFFFFFFFF00000000ul)]
    [InlineData(0x00000000ul, 0x00000000ul, 0xFFFFFFFFFFFFFFFFul)]
    public void Xnor_RV64_Computes_Correctly(ulong rs1, ulong rs2, ulong expected)
    {
        var s = new MachineState(SystemConfig.Rv64());
        s.Memory = new SystemBus(0x8000);
        s.Host = new MultiOSHandler { Silent = true };
        s.Registers.Write(5, rs1);
        s.Registers.Write(6, rs2);
        string code = ".text\nxnor t2, t0, t1\nebreak";
        new SourceAssembler(code) { TextBase = 0 }.Assemble(s);
        s.ProgramCounter = 0;
        new PipelinedRunner(s, new NullLogger()).Run(20);
        Assert.Equal(expected, s.Registers.Read(7));
    }
    #endregion

    #region CLZ Tests
    [Theory]
    [InlineData(0x0000000000000001ul, 63ul)]
    [InlineData(0x8000000000000000ul, 0ul)]
    [InlineData(0x0000000080000000ul, 32ul)]
    [InlineData(0x0000000000000000ul, 64ul)]
    public void Clz_RV64_Counts_Leading_Zeros(ulong rs1, ulong expected)
    {
        var s = new MachineState(SystemConfig.Rv64());
        s.Memory = new SystemBus(0x8000);
        s.Host = new MultiOSHandler { Silent = true };
        s.Registers.Write(5, rs1);
        string code = ".text\nclz t2, t0\nebreak";
        new SourceAssembler(code) { TextBase = 0 }.Assemble(s);
        s.ProgramCounter = 0;
        new PipelinedRunner(s, new NullLogger()).Run(20);
        Assert.Equal(expected, s.Registers.Read(7));
    }

    [Theory]
    [InlineData(0x00000001u, 31u)]
    [InlineData(0x80000000u, 0u)]
    [InlineData(0x00000000u, 32u)]
    public void Clzw_RV64_Counts_32bit_Leading_Zeros(uint rs1, uint expected)
    {
        var s = new MachineState(SystemConfig.Rv64());
        s.Memory = new SystemBus(0x8000);
        s.Host = new MultiOSHandler { Silent = true };
        s.Registers.Write(5, rs1);
        string code = ".text\nclzw t2, t0\nebreak";
        new SourceAssembler(code) { TextBase = 0 }.Assemble(s);
        s.ProgramCounter = 0;
        new PipelinedRunner(s, new NullLogger()).Run(20);
        Assert.Equal(expected, (uint)s.Registers.Read(7));
    }
    #endregion

    #region CTZ Tests
    [Theory]
    [InlineData(0x8000000000000000ul, 63ul)]
    [InlineData(0x0000000000000001ul, 0ul)]
    [InlineData(0x0000000000000000ul, 64ul)]
    public void Ctz_RV64_Counts_Trailing_Zeros(ulong rs1, ulong expected)
    {
        var s = new MachineState(SystemConfig.Rv64());
        s.Memory = new SystemBus(0x8000);
        s.Host = new MultiOSHandler { Silent = true };
        s.Registers.Write(5, rs1);
        string code = ".text\nctz t2, t0\nebreak";
        new SourceAssembler(code) { TextBase = 0 }.Assemble(s);
        s.ProgramCounter = 0;
        new PipelinedRunner(s, new NullLogger()).Run(20);
        Assert.Equal(expected, s.Registers.Read(7));
    }

    [Theory]
    [InlineData(0x80000000u, 31u)]
    [InlineData(0x00000001u, 0u)]
    [InlineData(0x00000000u, 32u)]
    public void Ctzw_RV64_Counts_32bit_Trailing_Zeros(uint rs1, uint expected)
    {
        var s = new MachineState(SystemConfig.Rv64());
        s.Memory = new SystemBus(0x8000);
        s.Host = new MultiOSHandler { Silent = true };
        s.Registers.Write(5, rs1);
        string code = ".text\nctzw t2, t0\nebreak";
        new SourceAssembler(code) { TextBase = 0 }.Assemble(s);
        s.ProgramCounter = 0;
        new PipelinedRunner(s, new NullLogger()).Run(20);
        Assert.Equal(expected, (uint)s.Registers.Read(7));
    }
    #endregion

    #region CPOP Tests
    [Theory]
    [InlineData(0x0000000000000000ul, 0ul)]
    [InlineData(0xFFFFFFFFFFFFFFFFul, 64ul)]
    [InlineData(0x5555555555555555ul, 32ul)]
    public void Cpop_RV64_Counts_Set_Bits(ulong rs1, ulong expected)
    {
        var s = new MachineState(SystemConfig.Rv64());
        s.Memory = new SystemBus(0x8000);
        s.Host = new MultiOSHandler { Silent = true };
        s.Registers.Write(5, rs1);
        string code = ".text\ncpop t2, t0\nebreak";
        new SourceAssembler(code) { TextBase = 0 }.Assemble(s);
        s.ProgramCounter = 0;
        new PipelinedRunner(s, new NullLogger()).Run(20);
        Assert.Equal(expected, s.Registers.Read(7));
    }

    [Theory]
    [InlineData(0x00000000u, 0u)]
    [InlineData(0xFFFFFFFFu, 32u)]
    public void Cpopw_RV64_Counts_32bit_Set_Bits(uint rs1, uint expected)
    {
        var s = new MachineState(SystemConfig.Rv64());
        s.Memory = new SystemBus(0x8000);
        s.Host = new MultiOSHandler { Silent = true };
        s.Registers.Write(5, rs1);
        string code = ".text\ncpopw t2, t0\nebreak";
        new SourceAssembler(code) { TextBase = 0 }.Assemble(s);
        s.ProgramCounter = 0;
        new PipelinedRunner(s, new NullLogger()).Run(20);
        Assert.Equal(expected, (uint)s.Registers.Read(7));
    }
    #endregion

    #region MAX/MIN Tests
    [Theory]
    [InlineData(5L, 10L, 10L)]
    [InlineData(-5L, -10L, -5L)]
    public void Max_RV64_Returns_Signed_Maximum(long rs1, long rs2, long expected)
    {
        var s = new MachineState(SystemConfig.Rv64());
        s.Memory = new SystemBus(0x8000);
        s.Host = new MultiOSHandler { Silent = true };
        s.Registers.Write(5, (ulong)rs1);
        s.Registers.Write(6, (ulong)rs2);
        string code = ".text\nmax t2, t0, t1\nebreak";
        new SourceAssembler(code) { TextBase = 0 }.Assemble(s);
        s.ProgramCounter = 0;
        new PipelinedRunner(s, new NullLogger()).Run(20);
        Assert.Equal((ulong)expected, s.Registers.Read(7));
    }

    [Theory]
    [InlineData(5ul, 10ul, 5ul)]
    public void Minu_RV64_Returns_Unsigned_Minimum(ulong rs1, ulong rs2, ulong expected)
    {
        var s = new MachineState(SystemConfig.Rv64());
        s.Memory = new SystemBus(0x8000);
        s.Host = new MultiOSHandler { Silent = true };
        s.Registers.Write(5, rs1);
        s.Registers.Write(6, rs2);
        string code = ".text\nminu t2, t0, t1\nebreak";
        new SourceAssembler(code) { TextBase = 0 }.Assemble(s);
        s.ProgramCounter = 0;
        new PipelinedRunner(s, new NullLogger()).Run(20);
        Assert.Equal(expected, s.Registers.Read(7));
    }
    #endregion

    #region SEXT/ZEXT Tests
    [Theory]
    [InlineData(0x80ul, 0xFFFFFFFFFFFFFF80ul)]
    [InlineData(0x7Ful, 0x7Ful)]
    public void SextB_RV64_SignExtends_Byte(ulong rs1, ulong expected)
    {
        var s = new MachineState(SystemConfig.Rv64());
        s.Memory = new SystemBus(0x8000);
        s.Host = new MultiOSHandler { Silent = true };
        s.Registers.Write(5, rs1);
        string code = ".text\nsext.b t2, t0\nebreak";
        new SourceAssembler(code) { TextBase = 0 }.Assemble(s);
        s.ProgramCounter = 0;
        new PipelinedRunner(s, new NullLogger()).Run(20);
        Assert.Equal(expected, s.Registers.Read(7));
    }

    [Theory]
    [InlineData(0xFFFFFFFFFFFFFFFFul, 0x000000000000FFFFul)]
    public void ZextH_RV64_ZeroExtends_Halfword(ulong rs1, ulong expected)
    {
        var s = new MachineState(SystemConfig.Rv64());
        s.Memory = new SystemBus(0x8000);
        s.Host = new MultiOSHandler { Silent = true };
        s.Registers.Write(5, rs1);
        string code = ".text\nzext.h t2, t0\nebreak";
        new SourceAssembler(code) { TextBase = 0 }.Assemble(s);
        s.ProgramCounter = 0;
        new PipelinedRunner(s, new NullLogger()).Run(20);
        Assert.Equal(expected, s.Registers.Read(7));
    }
    #endregion

    #region ROL/ROR Tests
    [Theory]
    [InlineData(0x8000000000000001ul, 1, 0x0000000000000003ul)]
    public void Rol_RV64_Rotates_Left(ulong rs1, int shamt, ulong expected)
    {
        var s = new MachineState(SystemConfig.Rv64());
        s.Memory = new SystemBus(0x8000);
        s.Host = new MultiOSHandler { Silent = true };
        s.Registers.Write(5, rs1);
        s.Registers.Write(6, (ulong)shamt);
        string code = ".text\nrol t2, t0, t1\nebreak";
        new SourceAssembler(code) { TextBase = 0 }.Assemble(s);
        s.ProgramCounter = 0;
        new PipelinedRunner(s, new NullLogger()).Run(20);
        Assert.Equal(expected, s.Registers.Read(7));
    }
    #endregion

    #region ORC.B / REV8 Tests
    [Theory]
    [InlineData(0x0102030405060708ul, 0xFFFFFFFFFFFFFFFFul)]
    public void OrcB_RV64_Sets_NonZero_Bytes_To_FF(ulong rs1, ulong expected)
    {
        var s = new MachineState(SystemConfig.Rv64());
        s.Memory = new SystemBus(0x8000);
        s.Host = new MultiOSHandler { Silent = true };
        s.Registers.Write(5, rs1);
        string code = ".text\norc.b t2, t0\nebreak";
        new SourceAssembler(code) { TextBase = 0 }.Assemble(s);
        s.ProgramCounter = 0;
        new PipelinedRunner(s, new NullLogger()).Run(20);
        Assert.Equal(expected, s.Registers.Read(7));
    }

    [Theory]
    [InlineData(0x0102030405060708ul, 0x0807060504030201ul)]
    public void Rev8_RV64_Reverses_Bytes(ulong rs1, ulong expected)
    {
        var s = new MachineState(SystemConfig.Rv64());
        s.Memory = new SystemBus(0x8000);
        s.Host = new MultiOSHandler { Silent = true };
        s.Registers.Write(5, rs1);
        string code = ".text\nrev8 t2, t0\nebreak";
        new SourceAssembler(code) { TextBase = 0 }.Assemble(s);
        s.ProgramCounter = 0;
        new PipelinedRunner(s, new NullLogger()).Run(20);
        Assert.Equal(expected, s.Registers.Read(7));
    }
    #endregion
}


