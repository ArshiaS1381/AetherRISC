using Xunit;
using Xunit.Abstractions;
using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Tests.Infrastructure;

namespace AetherRISC.Tests.Unit.Pipeline;

public class PipelineStressTests : PipelineTestFixture
{
    private readonly ITestOutputHelper _output;
    public PipelineStressTests(ITestOutputHelper output) => _output = output;

    [Fact]
    public void Branch_Flush_Integrity()
    {
        InitPipeline();
        Machine.Registers.Write(1, 10);
        Machine.Registers.Write(2, 10);
        
        // 1. BEQ x1, x2, +12 (Skip next 2 instructions)
        Assembler.Add(pc => Inst.Beq(1, 2, 12)); 
        
        // 2. ADDI x3, x0, 99 (BAD! Should be flushed)
        Assembler.Add(pc => Inst.Addi(3, 0, 99));
        
        // 3. ADDI x4, x0, 99 (BAD! Should be flushed)
        Assembler.Add(pc => Inst.Addi(4, 0, 99));
        
        // 4. ADDI x5, x0, 50 (Target: Should execute)
        Assembler.Add(pc => Inst.Addi(5, 0, 50));

        LoadProgram();
        
        Cycle(10);
        
        AssertReg(3, 0ul); 
        AssertReg(4, 0ul); 
        AssertReg(5, 50ul); 
    }

    [Fact]
    public void Store_Data_Forwarding_Check()
    {
        InitPipeline();
        Machine.Registers.Write(1, 0x100);
        
        Assembler.Add(pc => Inst.Addi(2, 0, 88));
        Assembler.Add(pc => Inst.Sw(1, 2, 0));
        Assembler.Add(pc => Inst.Lw(3, 1, 0));
        
        LoadProgram();
        Cycle(10);
        
        Assert.Equal(88u, Machine.Memory.ReadWord(0x100)); 
        AssertReg(3, 88ul); 
    }

    [Fact]
    public void Load_Use_Stall_Chain()
    {
        InitPipeline();
        Machine.Memory.WriteWord(0x100, 10);
        Machine.Memory.WriteWord(0x104, 20);
        Machine.Registers.Write(1, 0x100);
        
        Assembler.Add(pc => Inst.Lw(2, 1, 0));
        Assembler.Add(pc => Inst.Add(3, 2, 1));
        Assembler.Add(pc => Inst.Lw(4, 1, 4));
        Assembler.Add(pc => Inst.Add(5, 4, 3));
        
        LoadProgram();
        Cycle(15);
        
        AssertReg(2, 10ul);
        AssertReg(3, 10 + 0x100ul);
        AssertReg(4, 20ul);
        AssertReg(5, 20 + 10 + 0x100ul);
    }

    [Fact]
    public void Mixed_Forwarding_Sources()
    {
        InitPipeline();
        Assembler.Add(pc => Inst.Addi(1, 0, 10)); 
        Assembler.Add(pc => Inst.Addi(0, 0, 0));  
        Assembler.Add(pc => Inst.Addi(2, 0, 20)); 
        Assembler.Add(pc => Inst.Add(3, 1, 2));   
        
        LoadProgram();
        Cycle(10);
        
        AssertReg(3, 30ul);
    }

    [Fact]
    public void Fibonacci_Loop_Stress()
    {
        InitPipeline();
        Machine.Registers.Write(3, 5); 
        Machine.Registers.Write(2, 1);
        
        // Loop Start (Offset 0):
        // 1. BEQ x4, x3, End  
        // Offset Calculation: 
        // 0: BEQ
        // 4: ADD
        // 8: ADDI
        // 12: ADDI
        // 16: ADDI
        // 20: JAL
        // 24: NOP (End)
        // TARGET OFFSET = 24
        Assembler.Add(pc => Inst.Beq(4, 3, 24));
        
        // 2. ADD x5, x1, x2   
        Assembler.Add(pc => Inst.Add(5, 1, 2));
        
        // 3. ADDI x1, x2, 0   
        Assembler.Add(pc => Inst.Addi(1, 2, 0));
        
        // 4. ADDI x2, x5, 0   
        Assembler.Add(pc => Inst.Addi(2, 5, 0));
        
        // 5. ADDI x4, x4, 1   
        Assembler.Add(pc => Inst.Addi(4, 4, 1));
        
        // 6. JAL x0, -20      
        Assembler.Add(pc => Inst.Jal(0, -20));
        
        // End (Offset 24):
        // 7. NOP
        Assembler.Add(pc => Inst.Addi(0, 0, 0));
        
        LoadProgram();
        
        Cycle(100);
        
        AssertReg(1, 5ul); 
    }
}
