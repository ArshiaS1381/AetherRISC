using Xunit;
using Xunit.Abstractions;
using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Tests.Infrastructure;

namespace AetherRISC.Tests.Unit.Pipeline;

public class FactorialDebugProbe : PipelineTestFixture
{
    private readonly ITestOutputHelper _output;
    public FactorialDebugProbe(ITestOutputHelper output) => _output = output;

    [Fact]
    public void Cycle_Trace_Factorial()
    {
        InitPipeline();
        Machine.Registers.Write(2, 0x1000); // SP
        Machine.Registers.Write(10, 5);     // a0 = 5
        
        // --- Same Code as Stress Test ---
        Assembler.Add(pc => Inst.Jal(1, 8));       // 0: Call
        Assembler.Add(pc => Inst.Ebreak(0,0,1));   // 4: Halt (Should skip)
        // Factorial Label
        Assembler.Add(pc => Inst.Addi(2, 2, -16)); // 8: Grow Stack
        Assembler.Add(pc => Inst.Sw(1, 2, 8));     // 12: Save RA
        Assembler.Add(pc => Inst.Sw(10, 2, 0));    // 16: Save n
        Assembler.Add(pc => Inst.Addi(5, 0, 2));   // 20: Threshold
        Assembler.Add(pc => Inst.Bge(10, 5, 16));  // 24: Branch Recurse
        // Base Case
        Assembler.Add(pc => Inst.Addi(10, 0, 1));  // 28: Ret 1
        Assembler.Add(pc => Inst.Addi(2, 2, 16));  // 32: Pop
        Assembler.Add(pc => Inst.Jalr(0, 1, 0));   // 36: Ret
        // Recursive Step
        Assembler.Add(pc => Inst.Addi(10, 10, -1));// 40: n-1
        Assembler.Add(pc => Inst.Jal(1, -36));     // 44: Call
        Assembler.Add(pc => Inst.Lw(6, 2, 0));     // 48: Restore n (t1)
        Assembler.Add(pc => Inst.Lw(1, 2, 8));     // 52: Restore RA
        Assembler.Add(pc => Inst.Addi(2, 2, 16));  // 56: Pop
        Assembler.Add(pc => Inst.Mul(10, 10, 6));  // 60: Mul
        Assembler.Add(pc => Inst.Jalr(0, 1, 0));   // 64: Ret
        
        LoadProgram();

        _output.WriteLine("Cyc | PC   | Inst | a0(x10) | t1(x6) | sp(x2) | ra(x1)");
        
        for (int i = 0; i < 200; i++)
        {
            // Log state BEFORE cycle execution
            ulong pc = Machine.Registers.PC;
            ulong a0 = Machine.Registers[10];
            ulong t1 = Machine.Registers[6];
            ulong sp = Machine.Registers[2];
            ulong ra = Machine.Registers[1];

            // Try to guess instruction being executed (in Execute Stage)
            // Note: This is an approximation based on PC
            
            _output.WriteLine($"{i,3} | {pc,4} | ???? | {a0,7} | {t1,6} | {sp,6:X} | {ra,6}");

            if (Machine.Halted) break;
            Cycle();
        }
    }
}
