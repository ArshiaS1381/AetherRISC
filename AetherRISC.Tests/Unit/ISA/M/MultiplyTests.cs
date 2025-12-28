using Xunit;
using AetherRISC.Tests.Infrastructure;
using AetherRISC.Core.Architecture.Hardware.ISA;

namespace AetherRISC.Tests.Unit.ISA.M;

public class MultiplyTests : CpuTestFixture
{
    [Fact]
    public void Mul_Basic()
    {
        Init64();
        Assembler.Add(pc => Inst.Addi(1, 0, 10));
        Assembler.Add(pc => Inst.Addi(2, 0, -5));
        Assembler.Add(pc => Inst.Mul(3, 1, 2));
        Run(3);
        AssertReg(3, -50L);
    }

    [Fact]
    public void Mulh_SignedHighProduct()
    {
        Init64();
        Assembler.Add(pc => Inst.Addi(1, 0, -1));
        Assembler.Add(pc => Inst.Addi(2, 0, -1));
        Assembler.Add(pc => Inst.Mulh(3, 1, 2));
        Run(3);
        AssertReg(3, 0ul);
    }

    [Fact]
    public void Mulw_TruncatesTo32Bits_SignExtends()
    {
        Init64();

        // -1 * 1 => 32-bit result = 0xFFFFFFFF, sign-extended to 64 => 0xFFFFFFFFFFFFFFFF
        Assembler.Add(pc => Inst.Addi(1, 0, -1));
        Assembler.Add(pc => Inst.Addi(2, 0, 1));
        Assembler.Add(pc => Inst.Mulw(3, 1, 2));

        Run(3);

        AssertReg(3, 0xFFFFFFFFFFFFFFFFul);
    }
}
