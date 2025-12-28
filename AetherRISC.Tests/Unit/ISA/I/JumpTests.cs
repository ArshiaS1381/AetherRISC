using Xunit;
using AetherRISC.Tests.Infrastructure;
using AetherRISC.Core.Architecture.Hardware.ISA;

namespace AetherRISC.Tests.Unit.ISA.I;

public class JumpTests : CpuTestFixture
{
    [Fact]
    public void Jal_Simple_Forward_Jump()
    {
        Init64();
        // JAL x0, target
        Assembler.Add(pc => Inst.Jal(0, Assembler.To("target", pc)));
        Assembler.Add(pc => Inst.Addi(1, 0, 0xB)); // Skip
        Assembler.Add(pc => Inst.Addi(1, 0, 0xA), "target");
        
        Run(3);
        AssertReg(1, 0xAul);
    }

    [Fact]
    public void Jal_Call_Return_Pattern()
    {
        Init64();
        Assembler.Add(pc => Inst.Addi(2, 0, 100)); // SP=100
        Assembler.Add(pc => Inst.Jal(1, Assembler.To("func", pc)));
        Assembler.Add(pc => Inst.Addi(10, 0, 1), "end");
        Assembler.Add(pc => Inst.Nop(0, 0, 0));
        Assembler.Add(pc => Inst.Addi(5, 0, 42), "func");
        Assembler.Add(pc => Inst.Jalr(0, 1, 0)); // Return
        
        Run(10);
        AssertReg(5, 42ul);
        AssertReg(10, 1ul);
    }
}

