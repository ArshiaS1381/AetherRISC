using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.ISA.Types;
using Xunit;
using AetherRISC.Core.Architecture;
using AetherRISC.Core.Architecture.Hardware.ISA.Instructions;

namespace AetherRISC.Core.Tests.Architecture.ISA.Base;

public class ArithmeticTests
{
    private MachineState _state = new MachineState(SystemConfig.Rv64());

    private InstructionData Data(int rd, int rs1, int rs2, int imm) 
        => new InstructionData { Rd = rd, Rs1 = rs1, Rs2 = rs2, Immediate = (ulong)(long)imm };

    [Fact]
    public void ADDI_Should_Add_Immediate()
    {
        _state.Registers.Write(1, 10);
        var inst = new AddiInstruction(2, 1, -5); // x2 = x1 + (-5)
        inst.Execute(_state, Data(2, 1, 0, -5));
        
        Assert.Equal((ulong)5, _state.Registers.Read(2));
    }

    [Fact]
    public void ADD_Should_Sum_Registers()
    {
        _state.Registers.Write(1, 100);
        _state.Registers.Write(2, 200);
        var inst = new AddInstruction(3, 1, 2); // x3 = x1 + x2
        inst.Execute(_state, Data(3, 1, 2, 0));

        Assert.Equal((ulong)300, _state.Registers.Read(3));
    }

    [Fact]
    public void SUB_Should_Subtract_Registers()
    {
        _state.Registers.Write(1, 10);
        _state.Registers.Write(2, 3);
        var inst = new SubInstruction(3, 1, 2); // x3 = x1 - x2
        inst.Execute(_state, Data(3, 1, 2, 0));

        Assert.Equal((ulong)7, _state.Registers.Read(3));
    }

    [Fact]
    public void LUI_Should_Load_Upper_Immediate()
    {
        var inst = new LuiInstruction(1, 0x12345000);
        inst.Execute(_state, Data(1, 0, 0, 0x12345000));
        
        Assert.Equal((ulong)0x12345000, _state.Registers.Read(1));
    }
}



