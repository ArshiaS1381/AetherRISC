using Xunit;
using AetherRISC.Core.Architecture;
using AetherRISC.Core.Architecture.Memory;
using AetherRISC.Core.Helpers;

namespace AetherRISC.Core.Tests;

public class MemoryOpTests
{
    [Fact]
    public void Store_Then_Load_Should_Preserve_Data()
    {
        var state = new MachineState(SystemConfig.Rv64());
        state.Memory = new SystemBus(1024);

        state.Registers.Write(1, 0x100);
        state.Registers.Write(2, 0xDEADBEEF);

        // Manual Store Execution (Mocking Pipeline Logic)
        // SW x2, 4(x1) -> Mem[0x100 + 4] = 0xDEADBEEF
        state.Memory.WriteWord(0x104, (uint)state.Registers.Read(2));

        Assert.Equal((uint)0xDEADBEEF, state.Memory.ReadWord(0x104));

        // Manual Load Execution
        // LW x3, 4(x1) -> x3 = Mem[0x104]
        uint loaded = state.Memory.ReadWord(0x104);
        state.Registers.Write(3, loaded);

        Assert.Equal((ulong)0xDEADBEEF, state.Registers.Read(3));
    }
}
