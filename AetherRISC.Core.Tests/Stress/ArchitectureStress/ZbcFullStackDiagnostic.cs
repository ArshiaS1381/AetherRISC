using System;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Simulation.Runners;
using AetherRISC.Core.Architecture.Simulation;
using AetherRISC.Core.Helpers;
using AetherRISC.Core.Architecture.Hardware.Memory;
using Xunit;
using Xunit.Abstractions;

namespace AetherRISC.Core.Tests.Architecture.Stress;

public class ZbcFullStackDiagnostic
{
    private readonly ITestOutputHelper _output;
    public ZbcFullStackDiagnostic(ITestOutputHelper output) => _output = output;

    [Fact]
    public void Diagnostic_Full_Integration_Trace()
    {
        var s = new MachineState(SystemConfig.Rv64());
        s.Memory = new SystemBus(0x8000);
        s.Host = new MultiOSHandler { Silent = true };

        // Use values without bit 31 set to avoid sign-extension complexity
        string code = @"
            .text
            li t0, 0x12345678
            li t1, 0x1ABCDEF0
            clmul  t2, t0, t1
            clmulh t3, t0, t1
            clmulr t4, t0, t1
            ebreak
        ";

        var asm = new SourceAssembler(code) { TextBase = 0 };
        asm.Assemble(s);
        s.ProgramCounter = 0;

        var runner = new PipelinedRunner(s, new NullLogger());
        runner.Run(40);

        ulong t0 = s.Registers.Read(5);
        ulong t1 = s.Registers.Read(6);
        ulong t2 = s.Registers.Read(7);
        
        _output.WriteLine($"Final Register t0 (x5): 0x{t0:X}");
        _output.WriteLine($"Final Register t1 (x6): 0x{t1:X}");
        _output.WriteLine($"Final Register t2 (x7): 0x{t2:X}");

        var (expectedLo, _) = AetherRISC.Core.Architecture.Hardware.ISA.Extensions.B.Zbc.CarrylessMath.Clmul128(t0, t1);
        _output.WriteLine($"C# Reference Lo result: 0x{expectedLo:X}");

        Assert.Equal(expectedLo, t2);
    }
}
