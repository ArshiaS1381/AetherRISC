using Xunit;

namespace AetherRISC.SuperScalarTests;

public class ControlFlowTests
{
    [Fact] // Test 19
    public void BranchTaken_CorrectExecution()
    {
        var code = @"
            addi x1, x0, 1
            beq x1, x1, target
            addi x2, x0, 99  # Should skip
            target:
            addi x3, x0, 50
        ";
        var (runner, state) = TestHelper.Setup(code, 1);
        runner.Run(20);
        
        Assert.Equal(0ul, state.Registers[2]); // x2 not touched
        Assert.Equal(50ul, state.Registers[3]); // x3 set
    }

    [Fact] // Test 20
    public void BranchNotTaken_Fallthrough()
    {
        var code = @"
            addi x1, x0, 1
            addi x2, x0, 2
            beq x1, x2, target # 1 != 2, fallthrough
            addi x3, x0, 10
            target:
            addi x4, x0, 20
        ";
        var (runner, state) = TestHelper.Setup(code, 1);
        runner.Run(20);

        Assert.Equal(10ul, state.Registers[3]);
        Assert.Equal(20ul, state.Registers[4]);
    }

    [Fact] // Test 21
    public void Misprediction_FlushesPipeline()
    {
        // Static predictor = Not Taken.
        // Branch is Taken.
        // Pipeline fills with "addi x2" (ghost).
        // On Execute of BEQ, flush occurs.
        var code = @"
            beq x0, x0, target
            addi x2, x0, 666 # Ghost
            target:
            addi x3, x0, 777
        ";
        var (runner, state) = TestHelper.Setup(code, 1, earlyBranch: true);
        
        // Trace carefully
        runner.Run(15);

        Assert.Equal(0ul, state.Registers[2]); // Ghost instruction killed
        Assert.Equal(777ul, state.Registers[3]);
    }

    [Fact] // Test 22
    public void Jump_JAL_UpdatesPC()
    {
        var code = @"
            jal x1, target
            addi x2, x0, 99
            target:
            addi x3, x0, 1
        ";
        var (runner, state) = TestHelper.Setup(code, 1);
        runner.Run(10);
        
        Assert.Equal(0ul, state.Registers[2]);
        Assert.Equal(1ul, state.Registers[3]);
        Assert.NotEqual(0ul, state.Registers[1]); // RA set
    }
    
    [Fact] // Test 23
    public void Superscalar_Branch_InSlot1_Flushes()
    {
        // Slot 0: NOP
        // Slot 1: BEQ (Taken)
        // Next Fetch: Should come from Target.
        var code = @"
            nop
            beq x0, x0, target
            addi x5, x0, 99
            target:
            addi x6, x0, 1
        ";
        var (runner, state) = TestHelper.Setup(code, 2);
        runner.Run(10);
        Assert.Equal(0ul, state.Registers[5]);
        Assert.Equal(1ul, state.Registers[6]);
    }
}
