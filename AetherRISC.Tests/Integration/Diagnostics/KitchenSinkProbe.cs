using Xunit;
using Xunit.Abstractions;
using AetherRISC.Core.Assembler;
using AetherRISC.Tests.Infrastructure;
using AetherRISC.Tests.Unit.Pipeline;
using AetherRISC.Core.Architecture.Hardware.ISA.Encoding;

namespace AetherRISC.Tests.Integration.Diagnostics;

public class KitchenSinkProbe : PipelineTestFixture
{
    private readonly ITestOutputHelper _output;
    public KitchenSinkProbe(ITestOutputHelper output) => _output = output;

    [Fact]
    public void Run_Trace()
    {
        // Reduced case of the failing scenario in Kitchen Sink
        // Replicating Pass 2: [3, 1] -> Should swap to [1, 3]
        
        var source = @"
            .text
            li x7, 0x200
            li x11, 3
            li x12, 1
            sw x11, 0(x7)
            sw x12, 4(x7)
            
            # Trace Start
            # Simulating Loop where x9 might be stale
            li x9, 5        # Pre-load x9 with '5' (Simulate stale data)
            
            lw x8, 0(x7)    # Load 3
            lw x9, 4(x7)    # Load 1
            
            # If LW x9 fails/stalls improperly, x9 remains 5.
            # BLE 3, 5 -> 5 >= 3 -> True -> Branch -> FAIL
            # BLE 3, 1 -> 1 >= 3 -> False -> Fallthrough -> PASS
            
            ble x8, x9, skip
            
            # Fallthrough (Swap)
            sw x9, 0(x7)
            sw x8, 4(x7)
            li x3, 1
            j end
            
            skip:
            li x3, 0
            
            end:
            ebreak
        ";
        
        InitPipeline();
        var asm = new SourceAssembler(source);
        asm.Assemble(Machine);
        Machine.Registers.PC = asm.TextBase;

        // Run with tracing
        for(int i=0; i<30; i++)
        {
            Pipeline.Cycle();
            
            // Check for Branch in Execute
            var ex = Pipeline.Buffers.ExecuteMemory;
            if (!ex.IsEmpty && ex.DecodedInst != null && ex.DecodedInst.IsBranch)
            {
                 _output.WriteLine($"[Cycle {i}] Branch {ex.DecodedInst.Mnemonic} Taken={ex.BranchTaken}");
            }
            
            // Check Regs after WB (approximate)
            if (i == 20)
            {
                _output.WriteLine($"x8: {Machine.Registers.Read(8)} (Exp 3)");
                _output.WriteLine($"x9: {Machine.Registers.Read(9)} (Exp 1)");
            }
        }
        
        ulong val0 = Machine.Memory.ReadWord(0x200);
        _output.WriteLine($"[0]: {val0} (Exp 1)");
        
        Assert.Equal(1ul, val0);
    }
}
