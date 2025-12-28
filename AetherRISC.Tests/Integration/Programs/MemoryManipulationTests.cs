using Xunit;
using AetherRISC.Tests.Infrastructure;
using AetherRISC.Core.Architecture.Hardware.ISA;

namespace AetherRISC.Tests.Integration.Programs;

public class MemoryManipulationTests : CpuTestFixture
{
    [Fact]
    public void Memcpy_Copies_Array_Correctly()
    {
        Init64();
        uint src = 0x100;
        uint dst = 0x200;
        uint count = 4; // Words

        // Setup Source Data
        Memory.WriteWord(src + 0, 0x11111111);
        Memory.WriteWord(src + 4, 0x22222222);
        Memory.WriteWord(src + 8, 0x33333333);
        Memory.WriteWord(src + 12, 0x44444444);

        // Algorithm:
        // x10 = src, x11 = dst, x12 = count
        // loop:
        //   BEQ x12, x0, done
        //   LW  x5, 0(x10)
        //   SW  x5, 0(x11)
        //   ADDI x10, x10, 4
        //   ADDI x11, x11, 4
        //   ADDI x12, x12, -1
        //   J loop
        // done:

        Assembler.Add(pc => Inst.Addi(10, 0, (int)src));
        Assembler.Add(pc => Inst.Addi(11, 0, (int)dst));
        Assembler.Add(pc => Inst.Addi(12, 0, (int)count));

        Assembler.Add(pc => Inst.Beq(12, 0, Assembler.To("done", pc)), "loop");
        Assembler.Add(pc => Inst.Lw(5, 10, 0));
        Assembler.Add(pc => Inst.Sw(11, 5, 0));
        Assembler.Add(pc => Inst.Addi(10, 10, 4));
        Assembler.Add(pc => Inst.Addi(11, 11, 4));
        Assembler.Add(pc => Inst.Addi(12, 12, -1));
        Assembler.Add(pc => Inst.Jal(0, Assembler.To("loop", pc)));
        
        Assembler.Add(pc => Inst.Nop(0, 0, 0), "done");

        Run(100);

        // Verify Destination
        Assert.Equal(0x11111111u, Memory.ReadWord(dst + 0));
        Assert.Equal(0x22222222u, Memory.ReadWord(dst + 4));
        Assert.Equal(0x33333333u, Memory.ReadWord(dst + 8));
        Assert.Equal(0x44444444u, Memory.ReadWord(dst + 12));
    }
}


