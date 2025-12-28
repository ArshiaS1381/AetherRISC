using Xunit;
using AetherRISC.Tests.Infrastructure;
using AetherRISC.Core.Architecture.Hardware.ISA;

namespace AetherRISC.Tests.Unit.ISA.Zbb;

public class BasicBitManipulationTests : CpuTestFixture
{
    [Fact]
    public void Andn_Inverts_Second_Operand()
    {
        Init64();
        // x1 = 0b1111, x2 = 0b0101
        // x3 = x1 & ~x2 = 1111 & 1010 = 1010 (0xA)
        Assembler.Add(pc => Inst.Addi(1, 0, 0xF));
        Assembler.Add(pc => Inst.Addi(2, 0, 0x5));
        Assembler.Add(pc => Inst.Andn(3, 1, 2));

        Run(3);
        AssertReg(3, 0xAul);
    }

    [Fact]
    public void Clz_Counts_Leading_Zeros()
    {
        Init64();
        // Value: 1 -> 63 leading zeros in 64-bit
        Assembler.Add(pc => Inst.Addi(1, 0, 1));
        Assembler.Add(pc => Inst.Clz(2, 1, 0));  // Third param is rs2/imm placeholder

        Run(2);
        AssertReg(2, 63ul);
    }

    [Fact]
    public void Rev8_Swaps_Endianness()
    {
        Init64();
        ulong val = 0x0102030405060708;
        Memory.WriteDouble(0x100, val);

        Assembler.Add(pc => Inst.Ld(1, 0, 0x100));
        Assembler.Add(pc => Inst.Rev8(2, 1, 0));  // Third param is imm placeholder

        Run(2);
        AssertReg(2, 0x0807060504030201ul);
    }

    [Fact]
    public void OrcB_Sets_Bytes_To_FF()
    {
        Init64();
        // Input: 0x0000120000340000
        // Each non-zero byte becomes 0xFF
        Memory.WriteDouble(0x100, 0x0000120000340000);
        Assembler.Add(pc => Inst.Ld(1, 0, 0x100));
        Assembler.Add(pc => Inst.OrcB(2, 1, 0));  // Third param is imm placeholder

        Run(2);
        AssertReg(2, 0x0000FF0000FF0000ul);
    }
}
