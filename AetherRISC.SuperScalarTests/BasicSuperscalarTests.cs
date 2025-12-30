using Xunit;

namespace AetherRISC.SuperScalarTests;

public class BasicSuperscalarTests
{
    [Fact] // Test 1
    public void DualIssue_IndependentALU_ExecutesSameCycle()
    {
        var code = @"
            addi x1, x0, 10
            addi x2, x0, 20
        ";

        var (runner, state) = TestHelper.Setup(code, pipelineWidth: 2);
        runner.Step(3); // F -> D -> E

        var slot0 = runner.PipelineState.ExecuteMemory.Slots[0];
        var slot1 = runner.PipelineState.ExecuteMemory.Slots[1];

        Assert.True(slot0.Valid && slot1.Valid, "Both instructions should be in Execute stage.");
        Assert.Equal(10ul, slot0.AluResult);
        Assert.Equal(20ul, slot1.AluResult);
    }

    [Fact] // Test 2
    public void QuadIssue_IndependentALU_ExecutesSameCycle()
    {
        var code = @"
            addi x1, x0, 1
            addi x2, x0, 2
            addi x3, x0, 3
            addi x4, x0, 4
        ";

        var (runner, state) = TestHelper.Setup(code, pipelineWidth: 4);
        runner.Step(3);

        for (int i = 0; i < 4; i++)
        {
            var slot = runner.PipelineState.ExecuteMemory.Slots[i];
            Assert.True(slot.Valid, $"Slot {i} should be valid in 4-wide superscalar.");
            Assert.Equal((ulong)(i + 1), slot.AluResult);
        }
    }

    [Fact] // Test 3
    public void MixedTypes_LoadAndALU_ExecutesParallel()
    {
        // FIX: Load from offset 20 to avoid reading the instruction itself (Self-Modifying code issue)
        var code = @"
            lw x1, 20(x0)
            addi x2, x0, 55
        ";
        var (runner, state) = TestHelper.Setup(code, 2);
        
        // FIX: Write to Data address (20), do not overwrite code at 0
        state.Memory!.WriteWord(20, 99);

        runner.Step(3);

        var s0 = runner.PipelineState.ExecuteMemory.Slots[0];
        var s1 = runner.PipelineState.ExecuteMemory.Slots[1];

        Assert.True(s0.MemRead, $"Slot 0 should be MemRead. Raw Inst: {s0.RawInstruction:X}");
        Assert.Equal(55ul, s1.AluResult);
    }

    [Fact] // Test 4
    public void SimpleWriteback_UpdatesRegisters()
    {
        var code = @"addi x5, x0, 42";
        var (runner, state) = TestHelper.Setup(code, 2);
        runner.Run(10);
        Assert.Equal(42ul, state.Registers[5]);
    }
    
    [Theory] // Tests 5, 6, 7, 8
    [InlineData("add x3, x1, x2", 30)]
    [InlineData("sub x3, x1, x2", 10)]
    [InlineData("or x3, x1, x2", 30)]
    [InlineData("and x3, x1, x2", 0)]
    public void ALUTypes_VerifyComputation(string inst, int expected)
    {
        var code = $@"
            addi x1, x0, 20
            addi x2, x0, 10
            {inst}
        ";
        var (runner, state) = TestHelper.Setup(code, 1);
        runner.Run(10);
        Assert.Equal((ulong)expected, state.Registers[3]);
    }
}
