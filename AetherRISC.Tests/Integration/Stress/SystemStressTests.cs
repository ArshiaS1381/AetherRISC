using Xunit;
using AetherRISC.Tests.Infrastructure;
using AetherRISC.Core.Architecture.Hardware.ISA;

namespace AetherRISC.Tests.Integration.Stress;

public class SystemStressTests : CpuTestFixture
{
    [Fact]
    public void Memory_Checksum_Loop()
    {
        Init64();

        uint buf = 0x2000;
        Memory.WriteWord(buf + 0, 1);
        Memory.WriteWord(buf + 4, 2);
        Memory.WriteWord(buf + 8, 3);
        Memory.WriteWord(buf + 12, 4);

        // x10 = buf (needs LUI), x11 = 4, x12 = 0
        Assembler.Add(pc => Inst.Lui(10, 0x2000));
        Assembler.Add(pc => Inst.Addi(10, 10, 0));
        Assembler.Add(pc => Inst.Addi(11, 0, 4));
        Assembler.Add(pc => Inst.Addi(12, 0, 0));

        Assembler.Add(pc => Inst.Lw(5, 10, 0), "loop");
        Assembler.Add(pc => Inst.Xor(12, 12, 5));
        Assembler.Add(pc => Inst.Addi(10, 10, 4));
        Assembler.Add(pc => Inst.Addi(11, 11, -1));
        Assembler.Add(pc => Inst.Bne(11, 0, Assembler.To("loop", pc)));

        Run(100);

        AssertReg(12, 4ul);
    }

    [Fact]
    public void Random_Instructions_Fuzz_Test()
    {
        Init64();
        var rng = new System.Random(42);

        for (int i = 0; i < 100; i++)
        {
            int r = rng.Next(3);
            int rd = rng.Next(1, 31);
            int rs1 = rng.Next(0, 31);
            int rs2 = rng.Next(0, 31);

            if (r == 0) Assembler.Add(pc => Inst.Add(rd, rs1, rs2));
            else if (r == 1) Assembler.Add(pc => Inst.Sub(rd, rs1, rs2));
            else Assembler.Add(pc => Inst.Xor(rd, rs1, rs2));
        }

        Run(110);
    }
}
