using Xunit;
using AetherRISC.Tests.Infrastructure;
using AetherRISC.Core.Architecture.Hardware.ISA;

namespace AetherRISC.Tests.Unit.ISA.I;

public class ComparisonTests : CpuTestFixture
{
    [Fact]
    public void Slt_Signed_Negative_Vs_Positive()
    {
        Init64();
        // -1 (0xFF..FF) < 1 (0x00..01) is TRUE
        Assembler.Add(pc => Inst.Addi(1, 0, -1));
        Assembler.Add(pc => Inst.Addi(2, 0, 1));
        Assembler.Add(pc => Inst.Slt(3, 1, 2));
        
        Run(3);
        AssertReg(3, 1ul);
    }

    [Fact]
    public void Sltu_Unsigned_Negative_Vs_Positive()
    {
        Init64();
        // -1 (MaxUInt) < 1 is FALSE
        Assembler.Add(pc => Inst.Addi(1, 0, -1));
        Assembler.Add(pc => Inst.Addi(2, 0, 1));
        Assembler.Add(pc => Inst.Sltu(3, 1, 2));
        
        Run(3);
        AssertReg(3, 0ul);
    }
}
