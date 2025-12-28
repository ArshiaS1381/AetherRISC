using Xunit;
using AetherRISC.Tests.Infrastructure;
using AetherRISC.Core.Architecture.Hardware.ISA;

namespace AetherRISC.Tests.Unit.ISA.I;

public class LoadStoreTests : CpuTestFixture
{
    [Fact]
    public void Lw_Sw_RoundTrip_Works()
    {
        Init64();
        Memory.WriteWord(0x100, 0xDEADBEEF);
        Assembler.Add(pc => Inst.Lw(1, 0, 0x100));
        Assembler.Add(pc => Inst.Sw(0, 1, 0x200));
        Run(2);
        AssertReg(1, 0xFFFFFFFFDEADBEEF);
        Assert.Equal(0xDEADBEEF, Memory.ReadWord(0x200));
    }

    [Fact]
    public void Store_Load_With_Negative_Offset()
    {
        Init64();
        // Base = 0x100. Offset = -16. Addr = 0xF0.
        Assembler.Add(pc => Inst.Addi(1, 0, 0x100));
        Assembler.Add(pc => Inst.Addi(2, 0, 0x55));
        Assembler.Add(pc => Inst.Sd(1, 2, -16));
        
        Run(3);
        
        Assert.Equal(0x55ul, Memory.ReadDouble(0xF0));
    }
    
    [Fact]
    public void Lbu_ZeroExtends_Byte()
    {
        Init64();
        Memory.WriteByte(0x10, 0xFF);
        Assembler.Add(pc => Inst.Lbu(1, 0, 0x10));
        Run(1);
        AssertReg(1, 255ul); 
    }
}
