using Xunit;
using Xunit.Abstractions;
using AetherRISC.Core.Assembler;
using AetherRISC.Tests.Infrastructure;
using AetherRISC.Tests.Unit.Pipeline;

namespace AetherRISC.Tests.Integration.Diagnostics;

public class StallPersistenceProbe : PipelineTestFixture
{
    private readonly ITestOutputHelper _output;
    public StallPersistenceProbe(ITestOutputHelper output) => _output = output;

    [Fact]
    public void Probe_Stall_Persistence_In_Memory_Stage()
    {
        // SCENARIO: 
        // 1. LW x1, ...
        // 2. NOP (Allows LW to move from Execute -> Memory)
        // 3. ADDI x2, x1, 1 (Depends on x1. LW is now in Memory)
        
        var source = @"
            .text
            li x10, 0x100
            
            # 1. Load 0xAAAA from 0x100
            lw x1, 0(x10)
            
            # 2. Bubble filler (pushes LW to Memory Stage)
            nop
            
            # 3. Consumer (LW is in Memory, Data not ready in Register File)
            # MUST STALL here.
            addi x2, x1, 0
            
            ebreak
        ";
        
        InitPipeline();
        Machine.Memory.WriteWord(0x100, 0xAAAAAAAA);
        
        var asm = new SourceAssembler(source);
        asm.Assemble(Machine);
        Machine.Registers.PC = asm.TextBase;

        LoadProgram();

        Cycle(1); // Fetch LI
        Cycle(1); // Dec LI, Fetch LW
        Cycle(1); // Ex LI, Dec LW, Fetch NOP
        Cycle(1); // Mem LI, Ex LW, Dec NOP, Fetch ADDI
        
        // Cycle 5 Start:
        // exMem has LW (Moved from Ex). 
        // idEx has NOP. 
        // ifId has ADDI.
        // StructuralHazardUnit runs -> Sees LW in Mem vs ADDI in Fetch. Should Stall.
        
        Cycle(1); 
        
        // CRITICAL MOMENT: Check state immediately after Cycle 5 execution
        var ifId = Pipeline.Buffers.FetchDecode;
        var idEx = Pipeline.Buffers.DecodeExecute;
        
        _output.WriteLine($"[Cycle 5 Check] LW in Memory. NOP in Execute.");
        _output.WriteLine($"Is Fetch Stalled? {ifId.IsStalled}");
        _output.WriteLine($"Is Decode Flushed (Bubble)? {idEx.IsEmpty}");
        
        Assert.True(ifId.IsStalled, "Pipeline failed to stall when Load was in Memory Stage.");
        
        // Continue to completion
        Cycle(5);
        ulong x2 = Machine.Registers.Read(2);
        
        if (x2 == 0x100) 
             _output.WriteLine("FAILURE: x2 = 0x100. The Address was forwarded!");
        
        Assert.Equal(0xAAAAAAAAul, x2);
    }
}
