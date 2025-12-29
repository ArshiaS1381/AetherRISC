using Xunit;
using Xunit.Abstractions;
using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Tests.Infrastructure;
using AetherRISC.Core.Architecture.Hardware.Pipeline;

namespace AetherRISC.Tests.Unit.Pipeline;

public class ForwardingTests : PipelineTestFixture
{
    private readonly ITestOutputHelper _output;
    public ForwardingTests(ITestOutputHelper output) => _output = output;

    [Fact]
    public void Ex_To_Ex_Forwarding_Increments_Register()
    {
        // 1. ADDI x1, x0, 10
        // 2. ADDI x1, x1, 1  (Depends on 1, should get 10 forwarded from EX/MEM)
        // 3. ADDI x1, x1, 1  (Depends on 2, should get 11 forwarded from EX/MEM)
        // Final x1 should be 12.
        
        InitPipeline();
        
        Assembler.Add(pc => Inst.Addi(1, 0, 10)); // Cycle 0 fetch
        Assembler.Add(pc => Inst.Addi(1, 1, 1));  // Cycle 1 fetch
        Assembler.Add(pc => Inst.Addi(1, 1, 1));  // Cycle 2 fetch
        // Use ADDI x0, x0, 0 for NOP
        Assembler.Add(pc => Inst.Addi(0, 0, 0));  // Flush/Wait
        Assembler.Add(pc => Inst.Addi(0, 0, 0));
        Assembler.Add(pc => Inst.Addi(0, 0, 0));

        LoadProgram();

        // Cycle 0: Fetch Addi(10)
        Cycle(); 
        // Cycle 1: Fetch Addi(11), Dec Addi(10)
        Cycle();
        // Cycle 2: Fetch Addi(12), Dec Addi(11), Ex Addi(10) -> x1=10 (in EX/MEM)
        Cycle();
        
        // Cycle 3: ..., Dec Addi(12), Ex Addi(11). 
        // Data Hazard Check: Addi(11) needs x1. Addi(10) is in EX/MEM.
        // Should forward 10. Result 11.
        Cycle();

        // Cycle 4: ..., Ex Addi(12).
        // Data Hazard Check: Addi(12) needs x1. Addi(11) is in EX/MEM.
        // Should forward 11. Result 12.
        Cycle();
        
        Cycle(5); // Drain

        ulong res = Machine.Registers.Read(1);
        _output.WriteLine($"x1 = {res}");
        
        Assert.Equal(12ul, res);
    }

    [Fact]
    public void Mem_To_Ex_Forwarding_Works()
    {
        // 1. ADDI x1, x0, 10
        // 2. NOP (Bubble to push ADDI to MEM stage)
        // 3. ADD x2, x1, x0 (Should get 10 forwarded from MEM/WB)
        
        InitPipeline();
        Assembler.Add(pc => Inst.Addi(1, 0, 10));
        Assembler.Add(pc => Inst.Addi(0, 0, 0)); // NOP
        Assembler.Add(pc => Inst.Add(2, 1, 0));
        Assembler.Add(pc => Inst.Addi(0, 0, 0)); // NOP
        Assembler.Add(pc => Inst.Addi(0, 0, 0)); // NOP
        
        LoadProgram();
        
        Cycle(10);
        
        Assert.Equal(10ul, Machine.Registers.Read(2));
    }
    
    [Fact]
    public void Branch_Dependency_Forwarding()
    {
        // 1. ADDI x1, x0, 1
        // 2. BEQ x1, x0, FAIL (Depends on x1)
        // 3. ADDI x2, x0, 1 (Success)
        
        InitPipeline();
        Assembler.Add(pc => Inst.Addi(1, 0, 1));
        // Branch if x1 == 0 (Should be False). Target is +12
        Assembler.Add(pc => Inst.Beq(1, 0, 12)); 
        Assembler.Add(pc => Inst.Addi(2, 0, 1)); // Fallthrough Success
        Assembler.Add(pc => Inst.Ebreak(0, 0, 1));
        
        // FAIL Target (+12 from BEQ)
        Assembler.Add(pc => Inst.Addi(2, 0, 0)); // x2 = 0
        Assembler.Add(pc => Inst.Ebreak(0, 0, 1));
        
        LoadProgram();
        Cycle(10);
        
        ulong res = Machine.Registers.Read(2);
        
        if (res == 0) _output.WriteLine("Branch took FAIL path (x1 was read as 0)");
        else _output.WriteLine("Branch took SUCCESS path (x1 was read as 1)");

        Assert.Equal(1ul, res);
    }
}
