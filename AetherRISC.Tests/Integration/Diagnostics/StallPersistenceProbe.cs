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
        var source = @"
            .text
            li x10, 0x100
            lw x1, 0(x10)
            nop
            addi x2, x1, 0
            ebreak
        ";
        
        InitPipeline();
        Machine.Memory.WriteWord(0x100, 0xAAAAAAAA);
        
        var asm = new SourceAssembler(source);
        asm.Assemble(Machine);
        Machine.Registers.PC = asm.TextBase;

        // FIXED: Removed LoadProgram() which overwrote PC/Memory
        
        Cycle(4); // Advance to Cycle 5 start
        Cycle(1); 
        
        var ifId = Pipeline.Buffers.FetchDecode;
        
        _output.WriteLine($"Is Fetch Stalled? {ifId.IsStalled}");
        Assert.True(ifId.IsStalled, "Pipeline failed to stall when Load was in Memory Stage.");
        
        Cycle(5);
        ulong x2 = Machine.Registers.Read(2);
        
        // LW sign extends 0xAAAAAAAA (negative in 32-bit) to 0xFFFFAAAAAAAA
        Assert.Equal(0xFFFFFFFFAAAAAAAAul, x2);
    }
}
