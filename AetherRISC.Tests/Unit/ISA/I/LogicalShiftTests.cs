using Xunit;
using AetherRISC.Tests.Infrastructure;
using AetherRISC.Core.Architecture.Hardware.ISA;

namespace AetherRISC.Tests.Unit.ISA.I;

public class LogicalShiftTests : CpuTestFixture
{
    [Fact]
    public void Sll_ShiftLeft_MultipliesPowerOfTwo()
    {
        Init64();
        Assembler.Add(pc => Inst.Addi(1, 0, 2));
        Assembler.Add(pc => Inst.Addi(2, 0, 2));
        Assembler.Add(pc => Inst.Sll(3, 1, 2));
        Run(3);
        AssertReg(3, 8ul);
    }

    [Fact]
    public void Sra_ArithmeticShiftRight_PreservesSign()
    {
        Init64();
        Assembler.Add(pc => Inst.Addi(1, 0, -4));
        Assembler.Add(pc => Inst.Addi(2, 0, 1));
        Assembler.Add(pc => Inst.Sra(3, 1, 2));
        Run(3);
        AssertReg(3, -2L);
    }

    [Fact]
    public void Sll_Masks_Shift_Amount_RV64()
    {
        Init64();
        // Shift by 65 (0x41).
        // RV64 uses lower 6 bits (0x3F). 65 & 0x3F = 1.
        // Result should be 10 << 1 = 20. 
        // (If not masked, 10 << 65 would be 0).
        
        Assembler.Add(pc => Inst.Addi(1, 0, 10));
        Assembler.Add(pc => Inst.Addi(2, 0, 65));
        Assembler.Add(pc => Inst.Sll(3, 1, 2));
        
        Run(3);
        AssertReg(3, 20ul);
    }
}
