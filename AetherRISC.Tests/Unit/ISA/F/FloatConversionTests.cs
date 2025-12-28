using Xunit;
using AetherRISC.Tests.Infrastructure;
using AetherRISC.Core.Architecture.Hardware.ISA;
using System;

namespace AetherRISC.Tests.Unit.ISA.F;

public class FloatConversionTests : CpuTestFixture
{
    [Fact]
    public void NanBoxing_Single_Inside_Double_Register()
    {
        Init64();

        // Need x1 = bits(1.5f) = 0x3FC00000
        Assembler.Add(pc => Inst.Lui(1, 0x3FC00000));
        Assembler.Add(pc => Inst.Addi(1, 1, 0));

        Assembler.Add(pc => Inst.FmvWX(1, 1, 0));

        Run(3);

        double dVal = Machine.FRegisters.ReadDouble(1);
        ulong raw = BitConverter.DoubleToUInt64Bits(dVal);

        Assert.Equal(0xFFFFFFFFu, (uint)(raw >> 32));
        Assert.Equal((ulong)BitConverter.SingleToInt32Bits(1.5f), raw & 0xFFFFFFFF);
    }
}
