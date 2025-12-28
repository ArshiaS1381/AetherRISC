using Xunit;
using Xunit.Abstractions;
using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Tests.Infrastructure;
using System;

namespace AetherRISC.Tests.Unit.Pipeline;

public class ComplexExecutionTests : PipelineTestFixture
{
    private readonly ITestOutputHelper _output;
    public ComplexExecutionTests(ITestOutputHelper output) => _output = output;

    [Fact]
    public void Recursive_Factorial_Stress()
    {
        InitPipeline();
        Machine.Registers.Write(2, 0x1000); // SP
        Machine.Registers.Write(10, 5);     // a0 = 5
        
        // --- Code ---
        Assembler.Add(pc => Inst.Jal(1, 8)); // 0: Call
        Assembler.Add(pc => Inst.Ebreak(0, 0, 1)); // 4: Halt

        // --- Factorial Label (Offset 8) ---
        Assembler.Add(pc => Inst.Addi(2, 2, -16));  // 8: Grow stack
        Assembler.Add(pc => Inst.Sw(2, 1, 8));      // 12: Save RA (FIXED)
        Assembler.Add(pc => Inst.Sw(2, 10, 0));     // 16: Save n (FIXED)
        Assembler.Add(pc => Inst.Addi(5, 0, 2));    // 20: t0 = 2
        Assembler.Add(pc => Inst.Bge(10, 5, 16));   // 24: if a0 >= 2, goto 40
        
        // Base Case
        Assembler.Add(pc => Inst.Addi(10, 0, 1));   // 28: return 1
        Assembler.Add(pc => Inst.Addi(2, 2, 16));   // 32: pop
        Assembler.Add(pc => Inst.Jalr(0, 1, 0));    // 36: return
        
        // Recursive Step
        Assembler.Add(pc => Inst.Addi(10, 10, -1)); // 40: a0 = a0 - 1
        Assembler.Add(pc => Inst.Jal(1, -36));      // 44: call factorial
        Assembler.Add(pc => Inst.Lw(6, 2, 0));      // 48: t1 = saved n
        Assembler.Add(pc => Inst.Lw(1, 2, 8));      // 52: restore RA
        Assembler.Add(pc => Inst.Addi(2, 2, 16));   // 56: pop
        Assembler.Add(pc => Inst.Mul(10, 10, 6));   // 60: a0 = a0 * t1
        Assembler.Add(pc => Inst.Jalr(0, 1, 0));    // 64: return

        LoadProgram();
        
        Cycle(500);
        
        ulong result = Machine.Registers.Read(10);
        _output.WriteLine($"Factorial(5) Result: {result}");
        Assert.Equal(120ul, result);
    }

    [Fact]
    public void Bubble_Sort_Integration()
    {
        InitPipeline();
        uint baseAddr = 0x100;
        Machine.Memory.WriteWord(baseAddr + 0, 5);
        Machine.Memory.WriteWord(baseAddr + 4, 3);
        Machine.Memory.WriteWord(baseAddr + 8, 1);
        Machine.Memory.WriteWord(baseAddr + 12, 4);
        Machine.Memory.WriteWord(baseAddr + 16, 2);
        
        Assembler.Add(pc => Inst.Addi(1, 0, (int)baseAddr)); 
        Assembler.Add(pc => Inst.Addi(2, 0, 5));             
        
        // Outer Loop
        Assembler.Add(pc => Inst.Addi(2, 2, -1));
        Assembler.Add(pc => Inst.Beq(2, 0, 48)); 
        
        Assembler.Add(pc => Inst.Addi(4, 1, 0));
        Assembler.Add(pc => Inst.Addi(3, 2, 0));
        
        // Inner Loop
        Assembler.Add(pc => Inst.Lw(5, 4, 0));
        Assembler.Add(pc => Inst.Lw(6, 4, 4));
        
        Assembler.Add(pc => Inst.Blt(5, 6, 12));
        
        // Swap (FIXED: base first, then value)
        Assembler.Add(pc => Inst.Sw(4, 6, 0));
        Assembler.Add(pc => Inst.Sw(4, 5, 4));
        
        // NoSwap
        Assembler.Add(pc => Inst.Addi(4, 4, 4));
        Assembler.Add(pc => Inst.Addi(3, 3, -1));
        Assembler.Add(pc => Inst.Bne(3, 0, -28));
        
        Assembler.Add(pc => Inst.Jal(0, -48));
        
        // Exit
        Assembler.Add(pc => Inst.Ebreak(0, 0, 1)); 
        
        LoadProgram();
        
        Cycle(1000);
        
        Assert.Equal(1u, Machine.Memory.ReadWord(baseAddr + 0));
        Assert.Equal(2u, Machine.Memory.ReadWord(baseAddr + 4));
        Assert.Equal(3u, Machine.Memory.ReadWord(baseAddr + 8));
        Assert.Equal(4u, Machine.Memory.ReadWord(baseAddr + 12));
        Assert.Equal(5u, Machine.Memory.ReadWord(baseAddr + 16));
    }
}
