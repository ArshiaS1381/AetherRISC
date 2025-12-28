using Xunit;
using AetherRISC.Tests.Infrastructure;
using AetherRISC.Core.Architecture.Hardware.ISA;

namespace AetherRISC.Tests.Unit.ISA.Zba;

public class AddressGenerationTests : CpuTestFixture
{
    [Theory]
    [InlineData(10, 20, 40)]
    [InlineData(0, 5, 5)]
    public void Sh1add_Computes_Correctly(int rs1, int rs2, int expected)
    {
        Init64();
        Assembler.Add(pc => Inst.Addi(1, 0, rs1));
        Assembler.Add(pc => Inst.Addi(2, 0, rs2));
        Assembler.Add(pc => Inst.Sh1add(3, 1, 2));
        Run(3);
        AssertReg(3, (ulong)expected);
    }

    [Fact]
    public void Sh2add_Array_Indexing_Simulation()
    {
        Init64();
        int index = 5;

        // x1 = 5
        Assembler.Add(pc => Inst.Addi(1, 0, index));

        // x2 = 0x1000 requires LUI-style load.
        Assembler.Add(pc => Inst.Lui(2, 0x1000));
        Assembler.Add(pc => Inst.Addi(2, 2, 0));

        Assembler.Add(pc => Inst.Sh2add(3, 1, 2)); // (5<<2)+0x1000 = 0x1014

        Run(4);
        AssertReg(3, 0x1014ul);
    }

    [Fact]
    public void Add_Uw_ZeroExtends_Lower_32Bits()
    {
        Init64();

        Assembler.Add(pc => Inst.Addi(1, 0, -1));
        Assembler.Add(pc => Inst.Addi(2, 0, 10));
        Assembler.Add(pc => Inst.AddUw(3, 2, 1));

        Run(3);

        AssertReg(3, 0x100000009ul);
    }
}
