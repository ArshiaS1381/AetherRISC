using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.ISA.Types;
using Xunit;
using AetherRISC.Core.Architecture;
using AetherRISC.Core.Architecture.Hardware.ISA.Instructions;

namespace AetherRISC.Core.Tests.Architecture.ISA.Base;

public class LogicalTests
{
    private MachineState _state = new MachineState(SystemConfig.Rv64());
    private InstructionData Data(int rd, int rs1, int rs2, int imm) 
        => new InstructionData { Rd = rd, Rs1 = rs1, Rs2 = rs2, Immediate = (ulong)(long)imm };

    [Fact]
    public void XOR_Should_Xor_Bits()
    {
        _state.Registers.Write(1, 0b1100);
        _state.Registers.Write(2, 0b1010);
        var inst = new XorInstruction(3, 1, 2);
        inst.Execute(_state, Data(3, 1, 2, 0));
        
        Assert.Equal((ulong)0b0110, _state.Registers.Read(3));
    }

    [Fact]
    public void SLL_Should_Shift_Left()
    {
        _state.Registers.Write(1, 1);
        _state.Registers.Write(2, 4); // Shift by 4
        var inst = new SllInstruction(3, 1, 2);
        inst.Execute(_state, Data(3, 1, 2, 0));

        Assert.Equal((ulong)16, _state.Registers.Read(3));
    }

    [Fact]
    public void SLT_Should_Set_If_Less_Than()
    {
        _state.Registers.Write(1, 10);
        _state.Registers.Write(2, 20);
        var inst = new SltInstruction(3, 1, 2); // 10 < 20 ? 1 : 0
        inst.Execute(_state, Data(3, 1, 2, 0));

        Assert.Equal((ulong)1, _state.Registers.Read(3));
    }
}



