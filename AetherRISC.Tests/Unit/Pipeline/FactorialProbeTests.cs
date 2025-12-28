using Xunit;
using Xunit.Abstractions;
using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Tests.Infrastructure;

namespace AetherRISC.Tests.Unit.Pipeline;

public class FactorialProbeTests : PipelineTestFixture
{
    private readonly ITestOutputHelper _output;
    public FactorialProbeTests(ITestOutputHelper output) => _output = output;

    [Fact]
    public void Probe_Recursive_Factorial()
    {
        InitPipeline();
        Machine.Registers.Write(2, 0x1000); // SP
        Machine.Registers.Write(10, 5);     // a0 = 5
        
        // Exact same code as ComplexExecutionTests.Recursive_Factorial_Stress
        Assembler.Add(pc => Inst.Jal(1, 8));        // 0: Call
        Assembler.Add(pc => Inst.Ebreak(0, 0, 1)); // 4: Halt

        Assembler.Add(pc => Inst.Addi(2, 2, -16));  // 8: Grow stack
        Assembler.Add(pc => Inst.Sw(2, 1, 8));      // 12: Save RA
        Assembler.Add(pc => Inst.Sw(2, 10, 0));     // 16: Save n
        Assembler.Add(pc => Inst.Addi(5, 0, 2));    // 20: t0 = 2
        Assembler.Add(pc => Inst.Bge(10, 5, 16));   // 24: if a0 >= 2, goto 40
        
        Assembler.Add(pc => Inst.Addi(10, 0, 1));   // 28: return 1
        Assembler.Add(pc => Inst.Addi(2, 2, 16));   // 32: pop
        Assembler.Add(pc => Inst.Jalr(0, 1, 0));    // 36: return
        
        Assembler.Add(pc => Inst.Addi(10, 10, -1)); // 40: a0 = a0 - 1
        Assembler.Add(pc => Inst.Jal(1, -36));      // 44: call factorial
        Assembler.Add(pc => Inst.Lw(6, 2, 0));      // 48: t1 = saved n
        Assembler.Add(pc => Inst.Lw(1, 2, 8));      // 52: restore RA
        Assembler.Add(pc => Inst.Addi(2, 2, 16));   // 56: pop
        Assembler.Add(pc => Inst.Mul(10, 10, 6));   // 60: a0 = a0 * t1
        Assembler.Add(pc => Inst.Jalr(0, 1, 0));    // 64: return

        LoadProgram();

        _output.WriteLine("Cyc | PC   | Stage    | a0(x10) | t1(x6) | sp(x2) | ra(x1) | Mem[sp] | Mem[sp+8]");
        _output.WriteLine(new string('-', 95));

        int mulCount = 0;
        int returnCount = 0;

        for (int i = 0; i < 500; i++)
        {
            ulong pc = Machine.Registers.PC;
            ulong a0 = Machine.Registers[10];
            ulong t1 = Machine.Registers[6];
            ulong sp = Machine.Registers[2];
            ulong ra = Machine.Registers[1];
            
            // Read stack values
            uint memSp = Machine.Memory.ReadWord((uint)sp);
            uint memSpPlus8 = Machine.Memory.ReadWord((uint)(sp + 8));
            
            // Get current instruction info
            var decBuf = Pipeline.Buffers.DecodeExecute;
            var exBuf = Pipeline.Buffers.ExecuteMemory;
            var memBuf = Pipeline.Buffers.MemoryWriteback;
            
            string stage = decBuf.DecodedInst?.Mnemonic ?? (decBuf.IsEmpty ? "BUBBLE" : "???");
            
            // Track MUL executions
            if (exBuf.DecodedInst?.Mnemonic == "MUL")
            {
                mulCount++;
                _output.WriteLine($"*** MUL #{mulCount}: a0={a0}, t1={t1}, result will be {a0 * t1}");
            }
            
            // Track returns
            if (exBuf.DecodedInst?.Mnemonic == "JALR" && exBuf.BranchTaken)
            {
                returnCount++;
                _output.WriteLine($"*** RETURN #{returnCount}: jumping to {Machine.Registers.PC}, a0={a0}");
            }

            // Log every 5 cycles or on interesting events
            bool interesting = stage == "MUL" || stage == "JALR" || stage == "JAL" || 
                               stage == "BGE" || stage == "LW" || stage == "SW" ||
                               (i % 10 == 0);
            
            if (interesting)
            {
                _output.WriteLine($"{i,3} | {pc,4} | {stage,-8} | {a0,7} | {t1,6} | {sp,6:X} | {ra,6} | {memSp,7} | {memSpPlus8,9}");
            }

            if (Machine.Halted)
            {
                _output.WriteLine($"\n=== HALTED at cycle {i} ===");
                _output.WriteLine($"Final a0 (result): {Machine.Registers[10]}");
                _output.WriteLine($"Expected: 120");
                _output.WriteLine($"MUL count: {mulCount} (expected 4)");
                _output.WriteLine($"Return count: {returnCount}");
                break;
            }
            
            Cycle();
        }
        
        Assert.Equal(120ul, Machine.Registers.Read(10));
    }
}
