using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.ISA.Types;
using Xunit;
using AetherRISC.Core.Architecture;
using AetherRISC.Core.Architecture.Hardware.Memory;
using AetherRISC.Core.Architecture.Hardware.ISA.Instructions;

namespace AetherRISC.Core.Tests.Architecture.ISA.Base;

public class MemoryTests
{
    private MachineState _state;
    public MemoryTests()
    {
        _state = new MachineState(SystemConfig.Rv64());
        _state.Memory = new SystemBus(1024);
    }

    private InstructionData Data(int rd, int rs1, int rs2, int imm) 
        => new InstructionData { Rd = rd, Rs1 = rs1, Rs2 = rs2, Immediate = (ulong)(long)imm };

    [Fact]
    public void SD_And_LD_Should_Roundtrip()
    {
        _state.Registers.Write(1, 0x100); // Base Address
        _state.Registers.Write(2, 0xDEADBEEF); // Value

        // Store: Mem[0x100 + 8] = 0xDEADBEEF
        var store = new SdInstruction(1, 2, 8);
        store.Execute(_state, Data(0, 1, 2, 8));

        // Load: x3 = Mem[0x100 + 8]
        var load = new LdInstruction(3, 1, 8);
        load.Execute(_state, Data(3, 1, 0, 8));

        Assert.Equal((ulong)0xDEADBEEF, _state.Registers.Read(3));
    }
}



