using Xunit;
using Xunit.Abstractions;
using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Tests.Infrastructure;

namespace AetherRISC.Tests.Unit.Pipeline;

public class BranchIntegrationTests : PipelineTestFixture
{
    private readonly ITestOutputHelper _output;
    public BranchIntegrationTests(ITestOutputHelper output) => _output = output;

    [Fact]
    public void Verify_Forward_Branch_Offset()
    {
        // Setup:
        // 0: BEQ x0, x0, +16 (Always Taken)
        // ... skipped ...
        // 16: ADDI x2, x0, 99 (Target A)
        // 20: JAL x0, 0       (TRAP: Infinite Loop) - Prevents fallthrough
        // 24: ADDI x3, x0, 77 (Forbidden Zone)

        InitPipeline();
        
        // 0: Jump to 16
        Assembler.Add(pc => Inst.Beq(0, 0, 16)); 
        
        // 4, 8, 12: Filler
        Assembler.Add(pc => Inst.Addi(1, 0, 1));
        Assembler.Add(pc => Inst.Addi(1, 0, 2));
        Assembler.Add(pc => Inst.Addi(1, 0, 3));
        
        // 16: Target
        Assembler.Add(pc => Inst.Addi(2, 0, 99));
        
        // 20: Trap (Infinite Loop to prevent executing 24)
        Assembler.Add(pc => Inst.Jal(0, 0));

        // 24: Should NOT execute
        Assembler.Add(pc => Inst.Addi(3, 0, 77));
        
        LoadProgram();
        
        Cycle(10);
        
        // x2 should be 99 (Jump landed)
        Assert.Equal(99ul, Machine.Registers.Read(2));
        
        // x3 should be 0 (Fallthrough prevented)
        Assert.Equal(0ul, Machine.Registers.Read(3));
    }

    [Fact]
    public void Hazard_Branch_Interaction()
    {
        // GOAL: Verify branch works when operands need Forwarding.
        // 1. ADDI x1, x0, 10
        // 2. ADDI x2, x0, 10
        // 3. BEQ x1, x2, +8  (Depends on x1, x2 from Ex/Mem)
        // 4. ADDI x3, x0, 99 (Skip)
        // 5. ADDI x3, x0, 50 (Target)

        InitPipeline();
        
        Assembler.Add(pc => Inst.Addi(1, 0, 10));
        Assembler.Add(pc => Inst.Addi(2, 0, 10));
        
        // BEQ needs to read x1/x2. They are in pipeline (not in regfile yet).
        // Forwarding unit must feed ALU. ALU result feeds Branch Unit.
        Assembler.Add(pc => Inst.Beq(1, 2, 8));
        
        Assembler.Add(pc => Inst.Addi(3, 0, 99)); // Fail if executed
        Assembler.Add(pc => Inst.Addi(3, 0, 50)); // Success if executed
        
        LoadProgram();
        Cycle(10);
        
        Assert.Equal(50ul, Machine.Registers.Read(3));
    }
}
