using Xunit;
using Xunit.Abstractions;
using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Tests.Infrastructure;

namespace AetherRISC.Tests.Unit.Pipeline;

public class PipelineTortureTests : PipelineTestFixture
{
    private readonly ITestOutputHelper _output;
    public PipelineTortureTests(ITestOutputHelper output) => _output = output;

    [Fact]
    public void WAW_Priority_Correctness()
    {
        // TARGET: DataHazardUnit Priority Logic
        // Scenario: Two instructions write to x1 in the pipeline.
        // The consumer (ADD) must get the value from the YOUNGEST (most recent) producer.
        
        InitPipeline();
        
        // 1. ADDI x1, x0, 10 (Oldest - will be in MEM/WB)
        Assembler.Add(pc => Inst.Addi(1, 0, 10));
        
        // 2. ADDI x1, x0, 20 (Youngest - will be in EX/MEM)
        Assembler.Add(pc => Inst.Addi(1, 0, 20));
        
        // 3. ADD x2, x1, x0 (Consumer - in ID/EX)
        // If priority is wrong, x2 becomes 10. Correct is 20.
        Assembler.Add(pc => Inst.Add(2, 1, 0));
        
        LoadProgram();
        Cycle(10);
        
        _output.WriteLine($"x2 = {Machine.Registers.Read(2)} (Expected 20)");
        AssertReg(2, 20ul);
    }

    [Fact]
    public void Zero_Register_Immunity()
    {
        // TARGET: DataHazardUnit / ExecuteStage
        // Scenario: Writing to x0 should theoretically "produce" a result in the ALU.
        // If the Forwarding Unit doesn't check 'if (rd != 0)', it might forward 
        // that garbage ALU result to the next instruction reading x0.
        
        InitPipeline();
        
        // 1. ADDI x0, x0, 999 (ALU calculates 999. WB will ignore it. But Forwarding might not!)
        Assembler.Add(pc => Inst.Addi(0, 0, 999));
        
        // 2. ADD x1, x0, x0   (Should be 0 + 0 = 0)
        // If buggy, it forwards 999, making x1 = 1998 or 999.
        Assembler.Add(pc => Inst.Add(1, 0, 0));
        
        LoadProgram();
        Cycle(10);
        
        AssertReg(1, 0ul);
    }

    [Fact]
    public void Load_Use_Feeding_Branch()
    {
        // TARGET: StructuralHazardUnit + ControlHazardUnit interaction
        // Scenario: A loaded value is IMMEDIATELY used to decide a branch.
        // Requires: 1 Cycle Stall (Load-Use) -> Forwarding -> Branch Resolution.
        
        InitPipeline();
        Machine.Memory.WriteWord(0x100, 1); // True flag
        Machine.Registers.Write(1, 0x100);  // Pointer
        
        // 1. LW x2, 0(x1)  (Loads 1)
        Assembler.Add(pc => Inst.Lw(2, 1, 0));
        
        // 2. BNE x2, x0, Target (If 1 != 0, Jump). Depends on x2 immediately!
        Assembler.Add(pc => Inst.Bne(2, 0, 12));
        
        // 3. BAD: ADDI x3, x0, 666 (Should be skipped)
        Assembler.Add(pc => Inst.Addi(3, 0, 666));
        
        // 4. BAD: ADDI x3, x0, 666
        Assembler.Add(pc => Inst.Addi(3, 0, 666));
        
        // Target: ADDI x3, x0, 777
        Assembler.Add(pc => Inst.Addi(3, 0, 777));
        
        LoadProgram();
        Cycle(15);
        
        AssertReg(3, 777ul);
    }

    [Fact]
    public void Jump_Directly_To_Halt()
    {
        // TARGET: Pipeline Draining Logic
        // Scenario: A jump sends PC to an address containing EBREAK.
        // The fetch of EBREAK happens *after* the jump resolves in Execute.
        // This tests if the pipeline correctly transitions from "Flushing" to "Draining".
        
        InitPipeline();
        
        // 0: JAL x0, 12 (Jump to 12)
        Assembler.Add(pc => Inst.Jal(0, 12));
        
        // 4: Garbage (Should be flushed)
        Assembler.Add(pc => Inst.Addi(1, 0, 99));
        
        // 8: Garbage
        Assembler.Add(pc => Inst.Addi(1, 0, 99));
        
        // 12: EBREAK
        Assembler.Add(pc => Inst.Ebreak(0, 0, 1));
        
        LoadProgram();
        
        // Run loop manually to catch hangs
        int cycles = 0;
        while(!Machine.Halted && cycles < 20)
        {
            Cycle();
            cycles++;
        }
        
        Assert.True(Machine.Halted, "Machine failed to halt after Jump-to-Ebreak");
        AssertReg(1, 0ul); // Ensure garbage didn't execute
    }

    [Fact]
    public void Ping_Pong_Branches()
    {
        // TARGET: Control Hazard Unit Saturation
        // Scenario: Back-to-back branches.
        // 1. BEQ (Taken) -> Skips next instr
        // 2. JAL (At target) -> Jumps again
        
        InitPipeline();
        
        // 0: BEQ x0, x0, 8 (Jump to 8)
        Assembler.Add(pc => Inst.Beq(0, 0, 8));
        
        // 4: ADDI x1, x0, 99 (Skipped)
        Assembler.Add(pc => Inst.Addi(1, 0, 99));
        
        // 8: JAL x0, 8 (Jump to 16)
        Assembler.Add(pc => Inst.Jal(0, 8));
        
        // 12: ADDI x1, x0, 99 (Skipped)
        Assembler.Add(pc => Inst.Addi(1, 0, 99));
        
        // 16: ADDI x1, x0, 50 (Success)
        Assembler.Add(pc => Inst.Addi(1, 0, 50));
        
        LoadProgram();
        Cycle(20);
        
        AssertReg(1, 50ul);
    }
}
