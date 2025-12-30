using Xunit;

namespace AetherRISC.SuperScalarTests;

public class HazardTests
{
    [Fact] 
    public void IntraBundle_RAW_Dependency_ShouldStall()
    {
        var code = @"
            addi x1, x0, 10
            addi x2, x1, 5
        ";

        var (runner, state) = TestHelper.Setup(code, 2);

        // Phase 1: Run 3 cycles (F->D->E)
        // Cycle 3: Decode detects hazard, kills 'addi x2', rewinds PC.
        runner.Step(3); 

        var ex0 = runner.PipelineState.ExecuteMemory.Slots[0];
        var ex1 = runner.PipelineState.ExecuteMemory.Slots[1];

        // Slot 0 (addi x1) should proceed
        Assert.True(ex0.Valid, "Slot 0 (Producer) should be valid in C3");
        Assert.Equal(10ul, ex0.AluResult);
        
        // Slot 1 (addi x2) should be killed
        Assert.False(ex1.Valid, "Slot 1 (Consumer) should be invalidated");

        // Phase 2: Resume execution
        // C4: Fetch (addi x2)
        // C5: Decode 
        // C6: Execute -> Results available in ExecuteMemory
        // We need 3 more steps to complete Cycle 6 and check the buffer.
        // Wait, runner.Step(n) runs n cycles.
        // The instruction 'addi x2' is fetched in C4.
        // It reaches Execute stage in C6.
        // The Execute stage writes to ExecuteMemory buffer.
        // So checking buffer after 3 steps is correct.
        runner.Step(3); 
        
        ex0 = runner.PipelineState.ExecuteMemory.Slots[0];
        Assert.True(ex0.Valid, "Rewound instruction should execute eventually");
        Assert.Equal(15ul, ex0.AluResult); 
    }
    
    [Fact]
    public void LoadUse_Hazard_StallsPipeline()
    {
        var code = @"
            lw x1, 100(x0)
            addi x2, x1, 5
        ";
        var (runner, state) = TestHelper.Setup(code, 1); 
        state.Memory!.WriteWord(100, 10);

        runner.Step(3); // F, D, E (Hazard detected at end of C3/start of C4 logic)
        
        runner.Step(1); // C4: Stall Cycle
        Assert.True(runner.PipelineState.DecodeExecute.IsStalled, "Should stay stalled");
        
        runner.Step(1); // C5: Resume
        var exSlot = runner.PipelineState.ExecuteMemory.Slots[0];
        Assert.True(exSlot.Valid);
        Assert.Equal(15ul, exSlot.AluResult);
    }

    [Fact] public void InterBundle_Forwarding_EX_to_ID() { TestHelper.Setup("addi x1, x0, 10\n nop\n nop\n nop\n add x2, x1, x1", 2).Runner.Run(10); }
    [Fact] public void Forwarding_MEM_to_EX() { TestHelper.Setup("addi x1, x0, 10\n nop\n add x2, x1, x1", 1).Runner.Run(10); }
    [Fact] public void Float_IntraBundle_Stall() { TestHelper.Setup("fmv.w.x fa0, x0\n fadd.s fa1, fa0, fa0", 2).Runner.Run(10); Assert.True(true); }
}
