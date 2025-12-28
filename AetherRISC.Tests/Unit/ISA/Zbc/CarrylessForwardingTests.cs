using AetherRISC.Tests.Unit.Pipeline;
using Xunit;
using AetherRISC.Core.Architecture.Hardware.ISA;

namespace AetherRISC.Tests.Unit.ISA.Zbc;

public class CarrylessForwardingTests : PipelineTestFixture
{
    [Fact]
    public void Clmul_RAW_Hazard_Self_Dependency()
    {
        InitPipeline();

        // t0 = 0x1234 via LUI+ADDI (since 0x1234 > 2047)
        Assembler.Add(pc => Inst.Lui(5, 0x1000));
        Assembler.Add(pc => Inst.Addi(5, 5, 0x234));

        Assembler.Add(pc => Inst.Clmul(6, 5, 5));

        LoadProgram();
        Cycle(10);

        AssertReg(5, 0x1234ul);

        ulong res = Machine.Registers.Read(6);
        Assert.NotEqual(0ul, res);
    }
}
