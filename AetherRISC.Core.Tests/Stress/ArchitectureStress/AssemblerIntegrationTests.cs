using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Simulation.Runners;
using AetherRISC.Core.Helpers;
using AetherRISC.Core.Architecture.Hardware.Memory;
using Xunit;
using Xunit.Abstractions;

namespace AetherRISC.Core.Tests.Architecture.Stress;

public class AssemblerIntegrationTests
{
    private readonly ITestOutputHelper _output;
    public AssemblerIntegrationTests(ITestOutputHelper output) => _output = output;

    [Fact]
    public void Full_String_Program_Execution()
    {
        var state = new MachineState(SystemConfig.Rv64());
        state.Memory = new SystemBus(4096);
        state.ProgramCounter = 0; 

        string code = @"
            li t0, 10
            li t1, 20
            add t2, t0, t1
            sw t2, 512(zero)
            ebreak
        ";

        var assembler = new SourceAssembler(code) { TextBase = 0 };
        assembler.Assemble(state);
        
        var runner = new PipelinedRunner(state, new NullLogger());
        runner.Run(50);

        Assert.Equal(30ul, state.Registers.Read(7)); 
        Assert.Equal(30u, state.Memory.ReadWord(512));
    }
}
