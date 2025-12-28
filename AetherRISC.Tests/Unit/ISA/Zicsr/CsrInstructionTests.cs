using Xunit;
using AetherRISC.Tests.Infrastructure;
using AetherRISC.Core.Architecture.Hardware.ISA;

namespace AetherRISC.Tests.Unit.ISA.Zicsr;

public class CsrInstructionTests : CpuTestFixture
{
    [Fact]
    public void Csrrw_Swap_Values()
    {
        Init64();
        uint mepc = 0x341;

        Assembler.Add(pc => Inst.Addi(1, 0, 0xAA));
        Assembler.Add(pc => Inst.Addi(2, 0, 0x55));
        Assembler.Add(pc => Inst.Csrrw(0, 2, (int)mepc)); // init CSR
        Assembler.Add(pc => Inst.Csrrw(3, 1, (int)mepc)); // swap into x3

        Run(4);

        AssertReg(3, 0x55ul);
        Assert.Equal(0xAAul, Machine.Csr.Read(mepc));
    }

    [Fact]
    public void Csrrs_Set_Bits()
    {
        Init64();
        uint mstatus = 0x300;

        Assembler.Add(pc => Inst.Addi(1, 0, 1));
        Assembler.Add(pc => Inst.Csrrw(0, 1, (int)mstatus)); // init CSR=1

        Assembler.Add(pc => Inst.Addi(2, 0, 2));
        Assembler.Add(pc => Inst.Csrrs(3, 2, (int)mstatus)); // x3=old(1), CSR=3

        Run(4);

        AssertReg(3, 1ul);
        Assert.Equal(3ul, Machine.Csr.Read(mstatus));
    }
}
