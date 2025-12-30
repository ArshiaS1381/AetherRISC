using Xunit;
using System.Linq;
using AetherRISC.Core.Architecture.Simulation.Runners;
using AetherRISC.Core.Helpers;
using AetherRISC.Core.Architecture.Hardware.Pipeline;

namespace AetherRISC.SuperScalarTests;

public class AdvancedSuperscalarTests
{
    // ... [Previous Tests 1-3 omitted for brevity, keeping only the failing one fixed] ...

    [Fact] public void RAW_LoadUse_Superscalar() {
        // FIX: Safe memory offset
        var code = "lw x1, 40(x0)\n add x2, x1, x1";
        var (runner, state) = TestHelper.Setup(code, 2);
        state.Memory!.WriteWord(40, 5);
        runner.Run(15);
        Assert.Equal(10ul, state.Registers[2]);
    }

    [Fact] public void Saturation_8Wide_Nops() {
        var code = "nop\n nop\n nop\n nop\n nop\n nop\n nop\n nop";
        var (runner, state) = TestHelper.Setup(code, 8);
        runner.Step(1); runner.Step(1);
        int validCount = 0;
        for(int i=0; i<8; i++) if(runner.PipelineState.DecodeExecute.Slots[i].Valid) validCount++;
        Assert.Equal(8, validCount);
    }
}
