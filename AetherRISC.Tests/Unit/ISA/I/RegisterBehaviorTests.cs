using Xunit;
using AetherRISC.Tests.Infrastructure;
using AetherRISC.Core.Architecture.Hardware.ISA;

namespace AetherRISC.Tests.Unit.ISA.I;

public class RegisterBehaviorTests : CpuTestFixture
{
    [Fact]
    public void X0_Is_Immutable()
    {
        Init64();
        // Try writing to x0 via instruction
        Assembler.Add(pc => Inst.Addi(0, 0, 123));
        Run(1);
        AssertReg(0, 0ul);

        // Try writing via backdoor
        Machine.Registers.Write(0, 0xDEADBEEF);
        Assert.Equal(0ul, Machine.Registers.Read(0));
    }

    [Fact]
    public void Registers_Are_Independent()
    {
        Init64();
        // Write to x1, check x2
        Assembler.Add(pc => Inst.Addi(1, 0, 0xAA));
        Assembler.Add(pc => Inst.Addi(2, 0, 0xBB));
        Run(2);
        
        AssertReg(1, 0xAAul);
        AssertReg(2, 0xBBul);
    }
}
