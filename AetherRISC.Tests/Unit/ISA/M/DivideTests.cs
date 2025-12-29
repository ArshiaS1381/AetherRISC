using Xunit;
using AetherRISC.Tests.Infrastructure;
using AetherRISC.Core.Architecture.Hardware.ISA;

namespace AetherRISC.Tests.Unit.ISA.M;

public class DivideTests : CpuTestFixture
{
    [Fact]
    public void Div_ByZero_ReturnsMinusOne()
    {
        Init64();
        // RISC-V Spec: x / 0 = -1 (All bits set)
        Assembler.Add(pc => Inst.Addi(1, 0, 100));
        Assembler.Add(pc => Inst.Addi(2, 0, 0)); // Zero
        Assembler.Add(pc => Inst.Div(3, 1, 2));
        Run(3);
        AssertReg(3, 0xFFFFFFFFFFFFFFFFul);
    }

    [Fact]
    public void Div_Overflow_ReturnsMinInt()
    {
        Init64();
        // Signed Overflow: MinInt / -1 = MinInt
        // Setup registers directly to bypass potential Assembler/Encoder instruction limitations
        Machine.Registers.Write(1, 0x8000000000000000ul); // MinInt
        Machine.Registers.Write(2, 0xFFFFFFFFFFFFFFFFul); // -1
        
        Assembler.Add(pc => Inst.Div(3, 1, 2));
        
        Run(1);
        
        AssertReg(3, 0x8000000000000000ul);
    }

    [Fact]
    public void Divw_SignExtension_Check()
    {
        Init64();
        Assembler.Add(pc => Inst.Addi(1, 0, -100));
        Assembler.Add(pc => Inst.Addi(2, 0, 10));
        Assembler.Add(pc => Inst.Divw(3, 1, 2));
        Run(3);
        
        AssertReg(3, unchecked((ulong)-10L));
    }
}
