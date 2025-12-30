using System;
using AetherRISC.Core.Architecture.Hardware.Memory;
using AetherRISC.Core.Architecture.Hardware.Pipeline;
using AetherRISC.Core.Architecture.Simulation.Runners;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Assembler;
using AetherRISC.Core.Helpers;
using Xunit;

namespace AetherRISC.SuperScalarTests;

public static class TestHelper
{
    public static (PipelinedRunner Runner, MachineState State) Setup(string assembly, int pipelineWidth = 2, bool earlyBranch = true)
    {
        var config = SystemConfig.Rv64();
        var state = new MachineState(config)
        {
            // Allocate large memory to avoid bounds issues
            Memory = new SystemBus(0xFFFFFF) 
        };
        
        // Force code to address 0x0000 for simple debugging
        var asm = new SourceAssembler(assembly);
        asm.TextBase = 0x00000000; 
        asm.Assemble(state);
        
        // Ensure PC starts at 0
        state.ProgramCounter = 0x00000000;

        // Verify Memory Integrity immediately
        uint firstWord = state.Memory.ReadWord(0);
        if (firstWord == 0 && !string.IsNullOrWhiteSpace(assembly)) 
        {
             // This assertion helps us fail FAST if the assembler didn't emit anything
             // or if memory is broken.
             throw new Exception("Test Setup Failure: Memory at 0x0000 is empty (0x00000000). Assembler failed.");
        }

        var settings = new ArchitectureSettings
        {
            PipelineWidth = pipelineWidth,
            EnableEarlyBranchResolution = earlyBranch,
            BranchPredictorInitialValue = 1
        };

        var runner = new PipelinedRunner(state, new NullLogger(), "static", settings);
        
        return (runner, state);
    }
}
