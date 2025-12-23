using Xunit;
using AetherRISC.Core.Architecture;
using AetherRISC.Core.Helpers;
using AetherRISC.Core.Architecture.Memory;

namespace AetherRISC.Core.Tests.Architecture.System;

public class RvGComplianceTests
{
    private MachineState _state;
    public RvGComplianceTests() { 
        _state = new MachineState(SystemConfig.Rv64()); 
        _state.Memory = new SystemBus(1024);
    }

    [Fact]
    public void FMV_Bit_Coherency_Test()
    {
        // 1. Put raw bits in X1
        uint rawBits = 0x40490FDB; // Approx Pi
        _state.Registers.Write(1, rawBits);

        // 2. FMV.W.X (Move to F1)
        var fmvIn = new AetherRISC.Core.Architecture.ISA.Extensions.F.FmvWXInstruction(1, 1);
        fmvIn.Execute(_state, new InstructionData { Rd = 1, Rs1 = 1 });

        // 3. FMV.X.W (Move to X2)
        var fmvOut = new AetherRISC.Core.Architecture.ISA.Extensions.F.FmvXWInstruction(2, 1);
        fmvOut.Execute(_state, new InstructionData { Rd = 2, Rs1 = 1 });

        Assert.Equal((ulong)rawBits, _state.Registers.Read(2) & 0xFFFFFFFF);
    }
}
