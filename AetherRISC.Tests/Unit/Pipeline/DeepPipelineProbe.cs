using Xunit;
using Xunit.Abstractions;
using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Tests.Infrastructure;
using System.Linq;

namespace AetherRISC.Tests.Unit.Pipeline;

public class DeepPipelineProbe : PipelineTestFixture
{
    private readonly ITestOutputHelper _output;
    public DeepPipelineProbe(ITestOutputHelper output) => _output = output;

    [Fact]
    public void Monitor_Factorial_Execution()
    {
        Init64(); // Defaults to 1-wide in base PipelineTestFixture
        Machine.Registers.Write(2, 0x1000); // SP
        Machine.Registers.Write(10, 5);     // a0 = 5
        
        // --- Factorial Code ---
        Assembler.Add(pc => Inst.Jal(1, 8));       // 0: Call
        Assembler.Add(pc => Inst.Ebreak(0,0,1));   // 4: Halt
        Assembler.Add(pc => Inst.Addi(2, 2, -16)); // 8: Start - grow stack
        Assembler.Add(pc => Inst.Sw(2, 1, 8));     // 12: Save RA
        Assembler.Add(pc => Inst.Sw(2, 10, 0));    // 16: Save n
        Assembler.Add(pc => Inst.Addi(5, 0, 2));   // 20: t0 = 2
        Assembler.Add(pc => Inst.Bge(10, 5, 16));  // 24: Branch
        Assembler.Add(pc => Inst.Addi(10, 0, 1));  // 28: Base Case
        Assembler.Add(pc => Inst.Addi(2, 2, 16));  // 32: Pop stack
        Assembler.Add(pc => Inst.Jalr(0, 1, 0));   // 36: Return
        Assembler.Add(pc => Inst.Addi(10, 10, -1));// 40: Recurse
        Assembler.Add(pc => Inst.Jal(1, -36));     // 44: Call
        Assembler.Add(pc => Inst.Lw(6, 2, 0));     // 48: Restore n
        Assembler.Add(pc => Inst.Lw(1, 2, 8));     // 52: Restore RA
        Assembler.Add(pc => Inst.Addi(2, 2, 16));  // 56: Pop stack
        Assembler.Add(pc => Inst.Mul(10, 10, 6));  // 60: Mul
        Assembler.Add(pc => Inst.Jalr(0, 1, 0));   // 64: Return
        
        LoadProgram();

        _output.WriteLine("Cyc | PC   | Decode Op | Ex Result | Br? | a0(x10) | sp(x2)");
        _output.WriteLine(new string('-', 70));

        for (int i = 0; i < 250; i++)
        {
            ulong pc = Machine.Registers.PC;
            
            // USE HELPER PROPERTIES FOR SLOT 0
            var decOp = DecodeExecuteSlot;
            var exOp = ExecuteMemorySlot;
            
            string op = decOp.DecodedInst?.Mnemonic ?? (!decOp.Valid ? "BUBBLE" : "UNK");
            string res = !exOp.Valid ? " -- " : exOp.AluResult.ToString();
            string br = exOp.BranchTaken ? "YES" : " no";
            
            ulong a0 = Machine.Registers[10];
            ulong sp = Machine.Registers[2];

            _output.WriteLine($"{i,3} | {pc,4} | {op,-9} | {res,9} | {br} | {a0,7} | {sp,6:X}");

            if (Machine.Halted) break;
            Cycle();
        }
        
        Assert.Equal(120ul, Machine.Registers.Read(10));
    }
}
