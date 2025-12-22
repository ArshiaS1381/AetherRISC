using Xunit;
using AetherRISC.Core.Architecture;
using AetherRISC.Core.Architecture.Memory;
using AetherRISC.Core.Abstractions.Interfaces;
using AetherRISC.Core.Helpers;

namespace AetherRISC.Core.Tests;

public class TestHost : ISystemCallHandler
{
    // FIX: Use standard empty string literal
    public string Output { get; private set; } = "";
    public int ExitCode { get; private set; } = -1;

    public void PrintInt(long value) => Output += value.ToString();
    public void PrintString(string value) => Output += value;
    public void Exit(int code) => ExitCode = code;
}

public class SyscallTests
{
    [Fact]
    public void Should_Print_Hello_World()
    {
        var state = new MachineState(SystemConfig.Rv64());
        state.Memory = new SystemBus(1024);
        var host = new TestHost();
        state.Host = host;
        
        // Setup simple print test logic if needed later
    }
}
