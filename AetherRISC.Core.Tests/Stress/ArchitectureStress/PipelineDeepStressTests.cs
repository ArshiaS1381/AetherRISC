using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.Pipeline.Controller;
using AetherRISC.Core.Architecture.Hardware.Memory;
using AetherRISC.Core.Helpers;
using Xunit;

namespace AetherRISC.Core.Tests.Architecture.Stress;

public class PipelineDeepStressTests
{
    [Fact]
    public void Pipeline_RAW_Saturation_Test()
    {
        var state = new MachineState(SystemConfig.Rv64());
        state.Memory = new SystemBus(0x20000000);
        state.Registers.Write(2, 10);
        state.Registers.Write(3, 20);

        string code = @"
            .text
            add x1, x2, x3   # x1 = 30
            add x4, x1, x2   # x4 = 40 
            add x5, x4, x1   # x5 = 70 
            add x6, x5, x4   # x6 = 110 
            ebreak
        ";

        var assembler = new SourceAssembler(code) { TextBase = 0x00400000 };
        assembler.Assemble(state);
        
        state.ProgramCounter = 0x00400000;
        
        var pipe = new PipelineController(state);
        for(int i = 0; i < 30; i++) pipe.Cycle();

        Assert.Equal(110ul, state.Registers.Read(6));
    }
}
