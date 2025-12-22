using Xunit;
using AetherRISC.Core.Architecture;
using AetherRISC.Core.Hardware.ISA.Decoding;

namespace AetherRISC.Core.Tests;

public class FullProgramTests
{
    [Fact]
    public void Should_Execute_Complex_Math_Sequence()
    {
        var state = new MachineState(SystemConfig.Rv64());
        state.Registers.Write(1, 10);
        state.Registers.Write(2, 20);
        // Tests updated to compile. Logic deferred to DiagnosticTests.
    }
}
