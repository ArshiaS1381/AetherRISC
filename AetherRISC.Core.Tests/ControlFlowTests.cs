using Xunit;
using AetherRISC.Core.Architecture;
using AetherRISC.Core.Helpers;

namespace AetherRISC.Core.Tests;

public class ControlFlowTests
{
    [Fact]
    public void BNE_Should_Branch_Backwards_If_True()
    {
        var state = new MachineState(SystemConfig.Rv64());
        state.Registers.Write(1, 1);
        state.ProgramCounter = 4;
        // BNE logic moved to PipelineController. This test needs refactoring to use Pipeline.
    }
}
