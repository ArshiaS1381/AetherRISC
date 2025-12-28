using Xunit;
using AetherRISC.Tests.Infrastructure;
using AetherRISC.Core.Architecture.Hardware.ISA;

namespace AetherRISC.Tests.Unit.ISA.I;

public class EdgeCaseTests : CpuTestFixture
{
    [Fact]
    public void Sub_Wrap_On_Underflow()
    {
        Init64();
        // 0 - 1 = MaxUInt64 (0xFF...FF)
        Assembler.Add(pc => Inst.Addi(1, 0, 0));
        Assembler.Add(pc => Inst.Addi(2, 0, 1));
        Assembler.Add(pc => Inst.Sub(3, 1, 2));
        
        Run(3);
        
        AssertReg(3, ulong.MaxValue);
    }

    [Fact]
    public void Slt_Handles_Negative_Numbers()
    {
        Init64();
        // -1 (MaxUInt) < 1 is TRUE in Signed arithmetic
        Assembler.Add(pc => Inst.Addi(1, 0, -1));
        Assembler.Add(pc => Inst.Addi(2, 0, 1));
        Assembler.Add(pc => Inst.Slt(3, 1, 2));
        
        Run(3);
        
        AssertReg(3, 1ul);
    }

    [Fact]
    public void Sltu_Treats_Negative_As_Large_Positive()
    {
        Init64();
        // -1 (MaxUInt) < 1 is FALSE in Unsigned arithmetic
        Assembler.Add(pc => Inst.Addi(1, 0, -1));
        Assembler.Add(pc => Inst.Addi(2, 0, 1));
        Assembler.Add(pc => Inst.Sltu(3, 1, 2));
        
        Run(3);
        
        AssertReg(3, 0ul);
    }
}
