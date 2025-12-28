using Xunit;
using Xunit.Abstractions;
using AetherRISC.Core.Assembler;
using AetherRISC.Tests.Infrastructure;
using AetherRISC.Tests.Unit.Pipeline;

namespace AetherRISC.Tests.Integration.Diagnostics;

public class SortLogicProbe : PipelineTestFixture
{
    private readonly ITestOutputHelper _output;
    public SortLogicProbe(ITestOutputHelper output) => _output = output;

    [Fact]
    public void Probe_Specific_Swap_Hazard()
    {
        // Replicating the exact failure case: [3, 1]
        // Should become [1, 3]
        
        var source = @"
            .text
            li x7, 0x200
            li x11, 3
            li x12, 1
            sw x11, 0(x7)
            sw x12, 4(x7)
            
            # Load 3 and 1
            lw x8, 0(x7)
            lw x9, 4(x7)
            
            # BLE 3, 1 (False). Should NOT branch.
            # If it branches, x3 stays 0 (Failure).
            # If it falls through, x3 becomes 1 (Success).
            ble x8, x9, skip
            
            addi x3, x0, 1
            sw x9, 0(x7)
            sw x8, 4(x7)
            
            skip:
            ebreak
        ";
        
        InitPipeline();
        var asm = new SourceAssembler(source);
        asm.Assemble(Machine);
        Machine.Registers.PC = asm.TextBase;

        // Trace registers
        Cycle(20);

        ulong val0 = Machine.Memory.ReadWord(0x200);
        ulong val1 = Machine.Memory.ReadWord(0x204);
        ulong x3 = Machine.Registers.Read(3);

        _output.WriteLine($"x3 (Swap Flag): {x3} (Expected 1)");
        _output.WriteLine($"[0]: {val0} (Expected 1)");
        _output.WriteLine($"[1]: {val1} (Expected 3)");

        Assert.Equal(1ul, x3);
        Assert.Equal(1ul, val0);
    }
}
