using Xunit;
using AetherRISC.Core.Architecture;
using AetherRISC.Core.Architecture.Memory;
using AetherRISC.Core.Architecture.ISA.Base;

namespace AetherRISC.Core.Tests.Architecture.Granular;

public class GranularMemoryTests
{
    private MachineState _state;
    public GranularMemoryTests() {
        _state = new MachineState(SystemConfig.Rv64());
        _state.Memory = new SystemBus(1024);
    }
    
    private InstructionData Data(int rd, int rs1, int rs2, int imm) 
        => new InstructionData { Rd = rd, Rs1 = rs1, Rs2 = rs2, Immediate = (ulong)(long)imm, PC = 0 };

    [Fact]
    public void Store_Load_With_Negative_Offset()
    {
        _state.Registers.Write(1, 0x100);
        _state.Registers.Write(2, 0xCAFEBABE);
        
        var store = new SdInstruction(1, 2, -16);
        store.Execute(_state, Data(0, 1, 2, -16));
        
        // Use bang operator ! to assert Memory is not null
        Assert.Equal((ulong)0xCAFEBABE, _state.Memory!.ReadDouble(0xF0));
    }

    [Fact]
    public void LW_Should_Sign_Extend_To_64Bits()
    {
        _state.Memory!.WriteWord(0x10, 0xFFFFFFFF);
        _state.Registers.Write(1, 0x10);

        var load = new LwInstruction(2, 1, 0);
        load.Execute(_state, Data(2, 1, 0, 0));
        
        Assert.Equal(ulong.MaxValue, _state.Registers.Read(2));
    }
    
    [Fact]
    public void LW_Should_Not_Affect_Upper_Memory_Of_DoubleWord()
    {
        _state.Memory!.WriteDouble(0x20, 0x1111222233334444);
        _state.Registers.Write(1, 0x20);

        var load = new LwInstruction(2, 1, 0);
        load.Execute(_state, Data(2, 1, 0, 0));
        
        Assert.Equal((ulong)0x33334444, _state.Registers.Read(2));
    }
}

