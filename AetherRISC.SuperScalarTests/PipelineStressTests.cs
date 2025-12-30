using Xunit;

namespace AetherRISC.SuperScalarTests;

public class PipelineStressTests
{
    [Fact]
    public void Forwarding_ChainedDependencies()
    {
        // x1 = 10
        // x2 = x1 + 10 = 20
        // x3 = x2 + 10 = 30
        // x4 = x3 + 10 = 40
        // This requires rapid forwarding EX->EX and MEM->EX
        var code = @"
            addi x1, x0, 10
            addi x2, x1, 10
            addi x3, x2, 10
            addi x4, x3, 10
        ";
        var (runner, state) = TestHelper.Setup(code, 1);
        
        // Give it plenty of cycles
        runner.Run(20);

        Assert.Equal(10ul, state.Registers[1]);
        Assert.Equal(20ul, state.Registers[2]);
        Assert.Equal(30ul, state.Registers[3]);
        Assert.Equal(40ul, state.Registers[4]);
    }

    [Fact]
    public void Superscalar_Independent_Throughput()
    {
        // 4 instructions, width 4. Should retire all quickly.
        var code = @"
            addi x1, x0, 1
            addi x2, x0, 2
            addi x3, x0, 3
            addi x4, x0, 4
        ";
        var (runner, state) = TestHelper.Setup(code, 4); // 4-wide
        
        // Cycle 1: Fetch 4
        // Cycle 2: Decode 4
        // Cycle 3: Execute 4
        // Cycle 4: Mem 4
        // Cycle 5: WB 4
        runner.Run(5);

        Assert.Equal(1ul, state.Registers[1]);
        Assert.Equal(2ul, state.Registers[2]);
        Assert.Equal(3ul, state.Registers[3]);
        Assert.Equal(4ul, state.Registers[4]);
    }

    [Fact]
    public void Branch_Mispredict_CorrectsPC()
    {
        // Force Static Prediction (NT) on a Taken Branch
        // BEQ x0,x0 target (Taken)
        // Static predicts NT -> Fetches "addi x2" (garbage)
        // Execute detects mismatch -> flushes -> sets PC to target
        var code = @"
            beq x0, x0, target
            addi x2, x0, 999  # Should not commit
            target:
            addi x3, x0, 555
        ";
        var (runner, state) = TestHelper.Setup(code, 1);
        
        runner.Run(20);

        Assert.Equal(0ul, state.Registers[2]); // x2 stays 0
        Assert.Equal(555ul, state.Registers[3]); // x3 gets set
    }

    [Fact]
    public void ZeroRegister_IsNeverOverwritten()
    {
        // Attempt to write garbage to x0
        var code = @"
            addi x0, x0, 999
            add x1, x0, x0
        ";
        var (runner, state) = TestHelper.Setup(code, 2);
        runner.Run(10);

        Assert.Equal(0ul, state.Registers[0]);
        Assert.Equal(0ul, state.Registers[1]);
    }
}
