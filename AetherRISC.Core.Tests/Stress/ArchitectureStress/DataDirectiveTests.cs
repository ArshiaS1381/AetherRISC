using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Simulation.Runners;
using AetherRISC.Core.Helpers;
using AetherRISC.Core.Architecture.Hardware.Memory;
using Xunit;

namespace AetherRISC.Core.Tests.Architecture.Stress;

public class DataDirectiveTests
{
    [Fact]
    public void Assembler_Handles_Unaligned_Strings_And_Words()
    {
        var state = new MachineState(SystemConfig.Rv64());
        state.Memory = new SystemBus(0x20000000);
        
        string code = @"
            .data
            str:  .asciz ""Hi""
            .align 2
            val:  .word 0xDEADBEEF
            .text
            la t0, str
            lb t1, 0(t0)
            la t2, val
            lw t3, 0(t2)
            ebreak
        ";

        var assembler = new SourceAssembler(code);
        assembler.Assemble(state);
        
        var runner = new PipelinedRunner(state, new NullLogger());
        runner.Run(200);

        Assert.Equal((ulong)'H', state.Registers.Read(6)); 
        // Use unchecked to compare the lower 32 bits or accept the sign-extended version
        Assert.Equal(unchecked((ulong)(int)0xDEADBEEF), state.Registers.Read(28)); 
    }
}
