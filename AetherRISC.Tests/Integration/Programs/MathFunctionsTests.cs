using Xunit;
using AetherRISC.Tests.Infrastructure;
using AetherRISC.Core.Architecture.Hardware.ISA;

namespace AetherRISC.Tests.Integration.Programs;

public class MathFunctionsTests : CpuTestFixture
{
    [Fact]
    public void Factorial_5_Iterative()
    {
        Init64();
        // Res(x10)=1, N(x11)=5
        Assembler.Add(pc => Inst.Addi(10, 0, 1));
        Assembler.Add(pc => Inst.Addi(11, 0, 5));
        
        // Loop start
        Assembler.Add(pc => Inst.Beq(11, 0, Assembler.To("end", pc)), "loop");
        Assembler.Add(pc => Inst.Mul(10, 10, 11)); // x10 *= x11
        Assembler.Add(pc => Inst.Addi(11, 11, -1)); // x11--
        Assembler.Add(pc => Inst.Jal(0, Assembler.To("loop", pc)));
        
        Assembler.Add(pc => Inst.Nop(0, 0, 0), "end");

        Run(50); // Enough cycles

        AssertReg(10, 120ul); // 5! = 120
    }

    [Fact]
    public void Fibonacci_7th_Number()
    {
        Init64();
        // x1=0, x2=1, x3=7 (Target N)
        Assembler.Add(pc => Inst.Addi(1, 0, 0));
        Assembler.Add(pc => Inst.Addi(2, 0, 1));
        Assembler.Add(pc => Inst.Addi(3, 0, 7)); 

        // Loop
        Assembler.Add(pc => Inst.Add(4, 1, 2), "loop"); // temp = prev + curr
        Assembler.Add(pc => Inst.Addi(1, 2, 0));        // prev = curr
        Assembler.Add(pc => Inst.Addi(2, 4, 0));        // curr = temp
        Assembler.Add(pc => Inst.Addi(3, 3, -1));       // n--
        Assembler.Add(pc => Inst.Bne(3, 0, Assembler.To("loop", pc)));

        Run(100);

        // 7th Fib number is 21 (0, 1, 1, 2, 3, 5, 8, 13, 21)
        AssertReg(2, 21ul);
    }
}

