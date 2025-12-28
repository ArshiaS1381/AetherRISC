using System;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Simulation.Runners;
using AetherRISC.Core.Architecture.Simulation;
using AetherRISC.Core.Helpers;
using AetherRISC.Core.Architecture.Hardware.Memory;
using Xunit;
using Xunit.Abstractions;

namespace AetherRISC.Core.Tests.Architecture.Stress;

public class ZbcDeepTraceTests
{
    private readonly ITestOutputHelper _output;
    public ZbcDeepTraceTests(ITestOutputHelper output) => _output = output;

    [Fact]
    public void Trace_Clmul_Execution()
    {
        var s = new MachineState(SystemConfig.Rv64());
        s.Memory = new SystemBus(0x8000);
        s.Host = new MultiOSHandler { Silent = true };

        // We bypass LI expansion logic by using ADDI with small values first 
        // to see if the instruction itself is the issue.
        string code = @"
            .text
            addi t0, zero, 0x123
            addi t1, zero, 0x456
            clmul t2, t0, t1
            ebreak
        ";

        var asm = new SourceAssembler(code) { TextBase = 0 };
        asm.Assemble(s);
        s.ProgramCounter = 0;

        var runner = new PipelinedRunner(s, new NullLogger());
        runner.Run(30);

        ulong valT0 = s.Registers.Read(5);
        ulong valT1 = s.Registers.Read(6);
        ulong valT2 = s.Registers.Read(7);

        var (expectedLo, _) = AetherRISC.Core.Architecture.Hardware.ISA.Extensions.B.Zbc.CarrylessMath.Clmul128(valT0, valT1);

        _output.WriteLine($"t0: 0x{valT0:X}");
        _output.WriteLine($"t1: 0x{valT1:X}");
        _output.WriteLine($"t2 (Actual):   0x{valT2:X}");
        _output.WriteLine($"t2 (Expected): 0x{expectedLo:X}");

        Assert.Equal(expectedLo, valT2);
    }
}
