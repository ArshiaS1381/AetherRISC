using Xunit;
using AetherRISC.Core.Architecture.Hardware.ISA;

namespace AetherRISC.Tests.Unit.Pipeline;

public class HazardTests : PipelineTestFixture
{
    [Fact]
    public void RAW_Hazard_DataForwarding_Execute_To_Execute()
    {
        // Case: Result of Instr 1 is needed immediately by Instr 2
        // 1. ADDI x1, x0, 10  (Writes 10 to x1)
        // 2. ADD  x2, x1, x0  (Reads x1)
        
        InitPipeline();
        
        Assembler.Add(pc => Inst.Addi(1, 0, 10));
        Assembler.Add(pc => Inst.Add(2, 1, 0));
        
        LoadProgram();

        // Cycle 1: Fetch ADDI
        Cycle(); 
        
        // Cycle 2: Decode ADDI, Fetch ADD
        Cycle(); 
        
        // Cycle 3: Execute ADDI (x1=10 generated), Decode ADD (Need x1)
        // The DataHazard unit should detect this and forward 10 to the ALU inputs for ADD
        Cycle();
        
        // Cycle 4: Memory ADDI, Execute ADD
        // If forwarding worked, ADD calculated 10 + 0 = 10.
        Cycle(); 
        
        // Check latches to see if Execute latch has the correct result
        Assert.Equal(10ul, Pipeline.Buffers.ExecuteMemory.AluResult);
    }

    [Fact]
    public void Control_Hazard_Taken_Branch_Flushes_Pipeline()
    {
        InitPipeline();
        
        // 0x00: BEQ x0, x0, target (Taken)
        // 0x04: ADDI x1, x0, 99    (Should be flushed/skipped)
        // 0x08: target: ADDI x2, x0, 1
        
        Assembler.Add(pc => Inst.Beq(0, 0, Assembler.To("target", pc)));
        Assembler.Add(pc => Inst.Addi(1, 0, 99));
        Assembler.Add(pc => Inst.Addi(2, 0, 1), "target");
        
        LoadProgram();

        // C1: Fetch BEQ
        Cycle();
        // C2: Decode BEQ, Fetch ADDI(x1)
        Cycle();
        // C3: Exec BEQ. Detects Taken. Updates PC -> 0x08. Flushes Decode/Fetch latches.
        Cycle();
        
        // At this point, ADDI(x1) was in Decode. It should be turned into a Bubble (NOP).
        // The fetch stage should now be fetching 0x08.
        
        Cycle(); // Fetch Target (0x08)
        Cycle(); // Decode Target
        Cycle(); // Execute Target (x2 = 1)
        
        // Run a few more to commit
        Cycle(2);
        
        // x1 should still be 0 (skipped)
        AssertReg(1, 0ul);
        // x2 should be 1 (executed)
        AssertReg(2, 1ul);
    }
}
