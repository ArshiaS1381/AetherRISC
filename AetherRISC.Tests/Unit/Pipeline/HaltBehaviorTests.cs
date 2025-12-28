using Xunit;
using AetherRISC.Core.Architecture.Hardware.ISA;

namespace AetherRISC.Tests.Unit.Pipeline;

public class HaltBehaviorTests : PipelineTestFixture
{
    [Fact]
    public void Ebreak_Halts_And_Prevents_Further_State_Changes_When_Cycling()
    {
        InitPipeline();

        // 0: ADDI x10, x0, 120
        // 4: EBREAK (halt)
        // 8: ADDI x10, x0, 73  (must NOT commit after halt)
        Assembler.Add(pc => Inst.Addi(10, 0, 120));
        Assembler.Add(pc => Inst.Ebreak(0, 0, 1));
        Assembler.Add(pc => Inst.Addi(10, 0, 73));

        LoadProgram();

        Cycle(200);

        Assert.True(Machine.Halted);
        Assert.Equal(120ul, Machine.Registers.Read(10));
    }
}
