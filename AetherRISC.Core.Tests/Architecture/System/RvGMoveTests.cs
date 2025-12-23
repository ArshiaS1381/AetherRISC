using Xunit;
using AetherRISC.Core.Architecture;
using AetherRISC.Core.Helpers;
using AetherRISC.Core.Architecture.Memory;

namespace AetherRISC.Core.Tests.Architecture.System;

public class RvGMoveTests
{
    private MachineState _state;
    public RvGMoveTests() { 
        _state = new MachineState(SystemConfig.Rv64()); 
        _state.Memory = new SystemBus(1024);
    }

    [Fact]
    public void FMV_D_X_RoundTrip_PreservesBits()
    {
        // 1. Load a specific 64-bit pattern into X1
        ulong originalBits = 0xDEADBEEFCAFEBABEUL;
        _state.Registers.Write(1, originalBits);

        // 2. FMV.D.X f1, x1
        var fmvTo = new AetherRISC.Core.Architecture.ISA.Extensions.D.FmvDXInstruction(1, 1);
        fmvTo.Execute(_state, new InstructionData { Rd = 1, Rs1 = 1 });

        // 3. FMV.X.D x2, f1
        var fmvFrom = new AetherRISC.Core.Architecture.ISA.Extensions.D.FmvXDInstruction(2, 1);
        fmvFrom.Execute(_state, new InstructionData { Rd = 2, Rs1 = 1 });

        // Verify bits are unchanged
        Assert.Equal(originalBits, _state.Registers.Read(2));
    }
}
