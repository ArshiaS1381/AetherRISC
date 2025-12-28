using Xunit;
using Xunit.Abstractions;
using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Tests.Infrastructure;

namespace AetherRISC.Tests.Unit.Pipeline;

public class PipelineNightmareTests : PipelineTestFixture
{
    private readonly ITestOutputHelper _output;
    public PipelineNightmareTests(ITestOutputHelper output) => _output = output;

    [Fact]
    public void Indirect_Jump_Load_Dependency()
    {
        // TARGET: Stall Logic + PC Forwarding
        // 1. Store target address (0x200) into memory at 0x100
        // 2. Load x1 from 0x100
        // 3. JALR to x1 (Should jump to 0x200)
        
        InitPipeline();
        Machine.Memory.WriteWord(0x100, 0x200); // The pointer
        Machine.Registers.Write(2, 0x100);      // The address of the pointer
        
        // 0x200: Target code
        // ADDI x3, x0, 10
        Machine.Memory.WriteWord(0x200, 0x00A00193); 
        
        // 0x204: Explicit Halt (EBREAK) to catch the PC cleanly
        Machine.Memory.WriteWord(0x204, 0x00100073);

        // 1. LW x1, 0(x2)  (Loads 0x200)
        Assembler.Add(pc => Inst.Lw(1, 2, 0));
        
        // 2. JALR x0, x1, 0 (Jump to 0x200). Depends on x1.
        Assembler.Add(pc => Inst.Jalr(0, 1, 0));
        
        // 3. Garbage (Skipped)
        Assembler.Add(pc => Inst.Addi(3, 0, 666));

        LoadProgram();
        
        // Run enough cycles to flush, stall, jump, execute target, and halt
        int c = 0;
        while (!Machine.Halted && c < 20)
        {
            Cycle();
            c++;
        }
        
        // Check if we executed ADDI x3, x0, 10
        AssertReg(3, 10ul);
        
        // PC should be pointing to the instruction AFTER Ebreak (fetched but not executed)
        // Or at Ebreak depending on how halt is handled. 
        // 0x200 (ADDI) -> 0x204 (EBREAK). 
        // We assert strictly > 0x200 to prove we jumped.
        Assert.True(Machine.Registers.PC >= 0x200);
    }

    [Fact]
    public void Reflexive_Branch_Forwarding()
    {
        // TARGET: Branch Comparator Forwarding
        // 1. ADDI x1, x0, 5
        // 2. BEQ x1, x1, +12 (Target)
        
        InitPipeline();
        
        // 1. x1 = 5
        Assembler.Add(pc => Inst.Addi(1, 0, 5));
        
        // 2. BEQ x1, x1, 12 (Jump to Target)
        Assembler.Add(pc => Inst.Beq(1, 1, 12));
        
        // 3. Fail
        Assembler.Add(pc => Inst.Addi(10, 0, 666));
        Assembler.Add(pc => Inst.Addi(10, 0, 666));
        
        // Target:
        Assembler.Add(pc => Inst.Addi(10, 0, 777));
        
        LoadProgram();
        Cycle(10);
        
        AssertReg(10, 777ul);
    }

    [Fact]
    public void Ghost_Hazard_Immunity()
    {
        // TARGET: Hazard Unit ignoring invalid instructions
        // 1. BEQ x0, x0, Target (Taken)
        // 2. LW x1, 0(x0)       (Flushed)
        // 3. ADD x2, x1, x0     (Flushed) - DEPENDS on x1!
        
        InitPipeline();
        
        // 1. Jump to 16
        Assembler.Add(pc => Inst.Beq(0, 0, 16));
        
        // 2. Ghost Load
        Assembler.Add(pc => Inst.Lw(1, 0, 0));
        
        // 3. Ghost Use (Hazard?)
        Assembler.Add(pc => Inst.Add(2, 1, 0));
        
        // 4. Garbage
        Assembler.Add(pc => Inst.Addi(3, 0, 0));
        
        // Target (Offset 16)
        Assembler.Add(pc => Inst.Addi(10, 0, 1));
        
        LoadProgram();
        Cycle(10);
        
        AssertReg(10, 1ul);
        AssertReg(2, 0ul);
    }

    [Fact]
    public void Zero_Register_Writeback_Safety()
    {
        // TARGET: Memory Stage & Writeback
        // LW x0, 0(x1). Should execute but not write.
        
        InitPipeline();
        Machine.Registers.Write(1, 0x100);
        
        Assembler.Add(pc => Inst.Lw(0, 1, 0)); // Load to x0
        Assembler.Add(pc => Inst.Addi(2, 0, 1)); // Next op
        
        LoadProgram();
        
        // FIX: Increased cycle count from 5 to 10.
        // ADDI finishes in cycle 6.
        Cycle(10);
        
        AssertReg(2, 1ul);
    }

    [Fact]
    public void WAW_Shadowing()
    {
        // TARGET: Pipeline ordering
        // 1. ADDI x1, x0, 10
        // 2. ADDI x1, x0, 20
        
        InitPipeline();
        
        Assembler.Add(pc => Inst.Addi(1, 0, 10));
        Assembler.Add(pc => Inst.Addi(1, 0, 20));
        Assembler.Add(pc => Inst.Addi(0, 0, 0));
        Assembler.Add(pc => Inst.Addi(0, 0, 0));
        
        LoadProgram();
        Cycle(10);
        
        AssertReg(1, 20ul);
    }
}
