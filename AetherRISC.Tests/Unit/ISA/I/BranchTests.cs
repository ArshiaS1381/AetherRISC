using Xunit;
using AetherRISC.Tests.Infrastructure;
using AetherRISC.Core.Architecture.Hardware.ISA;

namespace AetherRISC.Tests.Unit.ISA.I;

public class BranchTests : CpuTestFixture
{
    [Fact]
    public void Beq_Taken_SkipsInstruction()
    {
        Init64();

        Assembler.Add(pc => Inst.Addi(1, 0, 10));
        Assembler.Add(pc => Inst.Addi(2, 0, 10));
        Assembler.Add(pc => Inst.Beq(1, 2, Assembler.To("target", pc)));
        Assembler.Add(pc => Inst.Addi(3, 0, 0x2AD));
        Assembler.Add(pc => Inst.Addi(3, 0, 0x2BC), "target");

        Run(5);

        AssertReg(3, 0x2BCul);
    }

    [Fact]
    public void Blt_Signed_Comparison()
    {
        Init64();

        Assembler.Add(pc => Inst.Addi(1, 0, -1));
        Assembler.Add(pc => Inst.Addi(2, 0, 1));
        Assembler.Add(pc => Inst.Blt(1, 2, 8));
        Assembler.Add(pc => Inst.Addi(3, 0, 0));
        Assembler.Add(pc => Inst.Addi(3, 0, 1));

        Run(5);

        AssertReg(3, 1ul);
    }
}
