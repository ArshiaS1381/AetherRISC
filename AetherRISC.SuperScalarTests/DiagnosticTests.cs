using Xunit;
using AetherRISC.Core.Architecture.Hardware.ISA;

namespace AetherRISC.SuperScalarTests;

public class DiagnosticTests
{
    [Fact]
    public void Stage0_Assembler_WritesToMemory()
    {
        // Sanity check: Does ADDI write bytes?
        // ADDI x1, x0, 1 -> 0x00100093
        var code = "addi x1, x0, 1";
        var (_, state) = TestHelper.Setup(code);

        uint val = state.Memory!.ReadWord(0);
        Assert.NotEqual(0u, val);
        Assert.Equal(0x00100093u, val);
    }

    [Fact]
    public void Stage1_Fetch_PopulatesBuffer()
    {
        // ADDI x1, x0, 1
        var code = "addi x1, x0, 1";
        var (runner, state) = TestHelper.Setup(code, 1);

        // Manually trigger ONE cycle
        runner.Step(1);

        // Check the FETCH -> DECODE buffer
        var fetchBuffer = runner.PipelineState.FetchDecode;
        var op = fetchBuffer.Slots[0];

        Assert.True(op.Valid, "Fetch slot should be valid after 1 cycle");
        Assert.Equal(0x00000000u, op.PC);
        Assert.Equal(0x00100093u, op.RawInstruction);
    }

    [Fact]
    public void Stage2_Decode_RecognizesInstruction()
    {
        var code = "addi x1, x0, 10";
        var (runner, state) = TestHelper.Setup(code, 1);

        // Run 2 cycles (Fetch -> Decode)
        runner.Step(1); // Fetch
        runner.Step(1); // Decode

        // Check the DECODE -> EXECUTE buffer
        var decBuffer = runner.PipelineState.DecodeExecute;
        var op = decBuffer.Slots[0];

        Assert.True(op.Valid, "Decode slot should be valid");
        Assert.NotNull(op.DecodedInst);
        Assert.Equal("ADDI", op.DecodedInst.Mnemonic);
        Assert.Equal(1, op.Rd);
        Assert.Equal(10, op.Immediate);
    }

    [Fact]
    public void Stage3_Execute_ComputesResult()
    {
        var code = "addi x1, x0, 50";
        var (runner, state) = TestHelper.Setup(code, 1);

        runner.Step(1); // Fetch
        runner.Step(1); // Decode
        runner.Step(1); // Execute

        var exBuffer = runner.PipelineState.ExecuteMemory.Slots[0];

        Assert.True(exBuffer.Valid, "Execute slot valid");
        Assert.Equal(50ul, exBuffer.AluResult);
    }

    [Fact]
    public void Stage4_Writeback_CommitsToRegFile()
    {
        var code = "addi x1, x0, 50";
        var (runner, state) = TestHelper.Setup(code, 1);

        // Cycle 1: Fetch
        // Cycle 2: Decode
        // Cycle 3: Execute
        // Cycle 4: Memory
        // Cycle 5: Writeback
        runner.Step(1);
        runner.Step(1);
        runner.Step(1);
        runner.Step(1);
        runner.Step(1);

        Assert.Equal(50ul, state.Registers[1]);
    }

    [Fact]
    public void Check_IllegalInstruction_Handling()
    {
        // Write 0x00000000 (Illegal) manually
        var (_, state) = TestHelper.Setup(""); // Empty code
        state.Memory!.WriteWord(0, 0); 
        
        var runner = new AetherRISC.Core.Architecture.Simulation.Runners.PipelinedRunner(
            state, new AetherRISC.Core.Helpers.NullLogger(), "static", 
            new AetherRISC.Core.Architecture.Hardware.Pipeline.ArchitectureSettings()
        );

        runner.Step(1); // Fetch 0
        runner.Step(1); // Decode (Should detect null -> EBREAK)
        runner.Step(1); // Execute
        runner.Step(1); // Memory
        runner.Step(1); // WB

        Assert.True(state.Halted, "Machine should halt on illegal instruction (0x0)");
    }
}
