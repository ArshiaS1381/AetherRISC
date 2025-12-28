using Xunit;
using AetherRISC.Tests.Infrastructure;
using AetherRISC.Core.Architecture.Hardware.ISA;

namespace AetherRISC.Tests.Unit.ISA.Zbc;

public class CarrylessMathTests : CpuTestFixture
{
    private static ulong RefClmul(ulong a, ulong b)
    {
        ulong output = 0;
        for (int i = 0; i < 64; i++)
        {
            if (((b >> i) & 1) == 1) output ^= (a << i);
        }
        return output;
    }

    [Fact]
    public void Clmul_Matches_Reference_Logic()
    {
        Init64();
        ulong a = 0x12345678;
        ulong b = 0x87654321;

        Memory.WriteDouble(0x100, a);
        Memory.WriteDouble(0x108, b);

        Assembler.Add(pc => Inst.Ld(1, 0, 0x100));
        Assembler.Add(pc => Inst.Ld(2, 0, 0x108));
        Assembler.Add(pc => Inst.Clmul(3, 1, 2));

        Run(3);

        AssertReg(3, RefClmul(a, b));
    }

    [Fact]
    public void Clmul_Identity_Xor()
    {
        Init64();

        // Build x1 = 0xABC safely:
        // 0xABC = 0x1000 - 0x544, and -0x544 fits signed 12-bit
        Assembler.Add(pc => Inst.Lui(1, 0x1000));
        Assembler.Add(pc => Inst.Addi(1, 1, -0x544));

        Assembler.Add(pc => Inst.Addi(2, 0, 1));
        Assembler.Add(pc => Inst.Clmul(3, 1, 2));

        Run(4);
        AssertReg(3, 0xABCul);
    }
}
