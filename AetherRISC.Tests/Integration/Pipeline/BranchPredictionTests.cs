using Xunit;
using Xunit.Abstractions;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Simulation.Runners;
using AetherRISC.Core.Assembler;
using AetherRISC.Core.Architecture.Hardware.Memory;
using AetherRISC.Core.Architecture.Simulation; // <-- FIXED: Added this namespace
using AetherRISC.Tests.Infrastructure;
using System.IO;

namespace AetherRISC.Tests.Integration.Pipeline;

public class BranchPredictionTests
{
    private readonly ITestOutputHelper _output;

    public BranchPredictionTests(ITestOutputHelper output)
    {
        _output = output;
    }

    private int RunSimulation(string source, string predictorType)
    {
        var config = SystemConfig.Rv64();
        var state = new MachineState(config);
        state.Memory = new SystemBus(1024 * 1024);
        state.Host = new MultiOSHandler { Output = new StringWriter(), Silent = true };

        var assembler = new SourceAssembler(source) { TextBase = 0x80000000 };
        assembler.Assemble(state);
        state.ProgramCounter = 0x80000000;

        var runner = new PipelinedRunner(state, new NullLogger(), predictorType);
        
        int cycles = 0;
        // Run until Halt or Timeout
        while (!state.Halted && cycles < 10000)
        {
            runner.Step(cycles);
            cycles++;
        }
        return cycles;
    }

    [Fact]
    public void Bimodal_Should_Outperform_Static_In_Tight_Loops()
    {
        var source = @"
            .text
            li t0, 50       # Loop count
            li t1, 0        # Counter
            
        loop:
            addi t1, t1, 1
            bne t1, t0, loop
            
            ebreak
        ";

        int staticCycles = RunSimulation(source, "static");
        int bimodalCycles = RunSimulation(source, "bimodal");

        _output.WriteLine($"Static Cycles:  {staticCycles}");
        _output.WriteLine($"Bimodal Cycles: {bimodalCycles}");

        Assert.True(bimodalCycles < staticCycles, "Bimodal predictor should result in fewer cycles due to fewer flushes.");
    }
}
