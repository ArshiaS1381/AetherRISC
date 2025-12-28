using Xunit;
using AetherRISC.Tests.Infrastructure;
using AetherRISC.Core.Architecture.Hardware.ISA;

namespace AetherRISC.Tests.Unit.ISA.F;

public class FloatSingleTests : CpuTestFixture
{
    [Fact]
    public void Fadd_S_Basic()
    {
        Init64();

        Assembler.Add(pc => Inst.Addi(1, 0, 2));
        Assembler.Add(pc => Inst.FcvtSW(1, 1, 0)); // f1 = 2.0
        Assembler.Add(pc => Inst.FaddS(2, 1, 1));  // f2 = 4.0

        Run(3);

        Assert.Equal(4.0f, Machine.FRegisters.ReadSingle(2));
    }

    [Fact]
    public void Fmv_WX_BitTransfer()
    {
        Init64();

        // Want x1 = 0x40490FDB (pi float bits)
        // Use LUI + ADDI with a negative low 12 (0xFDB = -37)
        Assembler.Add(pc => Inst.Lui(1, 0x40491000));
        Assembler.Add(pc => Inst.Addi(1, 1, -37));

        Assembler.Add(pc => Inst.FmvWX(1, 1, 0));

        Run(3);

        float val = Machine.FRegisters.ReadSingle(1);
        Assert.InRange(val, 3.1415, 3.1416);
    }
}
