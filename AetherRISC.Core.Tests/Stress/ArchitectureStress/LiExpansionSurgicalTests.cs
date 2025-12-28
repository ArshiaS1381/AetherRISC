using System;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Simulation.Runners;
using AetherRISC.Core.Architecture.Simulation;
using AetherRISC.Core.Helpers;
using AetherRISC.Core.Architecture.Hardware.Memory;
using Xunit;
using Xunit.Abstractions;

namespace AetherRISC.Core.Tests.Architecture.Stress;

public class LiExpansionSurgicalTests
{
    private readonly ITestOutputHelper _output;
    public LiExpansionSurgicalTests(ITestOutputHelper output) => _output = output;

    [Fact]
    public void Test_LI_Sign_Extension_Boundary()
    {
        var s = new MachineState(SystemConfig.Rv64());
        s.Memory = new SystemBus(0x8000);
        s.Host = new MultiOSHandler { Silent = true };

        // 0x9ABCDEF0 has bit 31 set. 
        // In a 32-bit signed context, this is negative.
        // In RV64, LI should ideally result in 0x000000009ABCDEF0 or 0xFFFFFFFF9ABCDEF0 
        // depending on how the assembler expands it.
        string code = @"
            .text
            li t0, 0x9ABCDEF0
            ebreak
        ";

        var asm = new SourceAssembler(code) { TextBase = 0 };
        asm.Assemble(s);
        s.ProgramCounter = 0;

        var runner = new PipelinedRunner(s, new NullLogger());
        runner.Run(20);

        ulong valT0 = s.Registers.Read(5);
        _output.WriteLine($"LI 0x9ABCDEF0 resulted in: 0x{valT0:X}");
        
        // If this is 0xFFFFFFFF9ABCDEF0, then the CLMUL will fail because 
        // carryless multiplication by a sign-extended negative-looking number 
        // produces a vastly different bit-pattern than the zero-extended version.
        Assert.True(valT0 == 0x9ABCDEF0ul || valT0 == 0xFFFFFFFF9ABCDEF0ul, 
            $"Value was unexpected: 0x{valT0:X}");
    }
}
