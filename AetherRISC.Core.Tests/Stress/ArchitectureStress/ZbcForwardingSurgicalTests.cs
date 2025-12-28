using System;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Simulation.Runners;
using AetherRISC.Core.Architecture.Simulation;
using AetherRISC.Core.Helpers;
using AetherRISC.Core.Architecture.Hardware.Memory;
using Xunit;
using Xunit.Abstractions;

namespace AetherRISC.Core.Tests.Architecture.Stress;

public class ZbcForwardingSurgicalTests
{
    private readonly ITestOutputHelper _output;
    public ZbcForwardingSurgicalTests(ITestOutputHelper output) => _output = output;

    [Fact]
    public void Test_Clmul_With_Immediate_RAW_Hazard()
    {
        var s = new MachineState(SystemConfig.Rv64());
        s.Memory = new SystemBus(0x8000);
        s.Host = new MultiOSHandler { Silent = true };

        // We create a tight RAW hazard. 
        // t0 is written in instruction 1.
        // t0 is read by clmul in instruction 2.
        // This MUST trigger forwarding from EX/MEM or MEM/WB.
        string code = @"
            .text
            li t0, 0x1234
            clmul t1, t0, t0
            ebreak
        ";

        var asm = new SourceAssembler(code) { TextBase = 0 };
        asm.Assemble(s);
        s.ProgramCounter = 0;

        var runner = new PipelinedRunner(s, new NullLogger());
        runner.Run(25);

        ulong valT0 = s.Registers.Read(5);
        ulong valT1 = s.Registers.Read(6);

        // Reference math
        var (expected, _) = AetherRISC.Core.Architecture.Hardware.ISA.Extensions.B.Zbc.CarrylessMath.Clmul128(valT0, valT0);

        _output.WriteLine($"t0: 0x{valT0:X}");
        _output.WriteLine($"t1 (Actual):   0x{valT1:X}");
        _output.WriteLine($"t1 (Expected): 0x{expected:X}");

        Assert.Equal(expected, valT1);
    }
}
