using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.ISA.Types;
using Xunit;
using AetherRISC.Core.Architecture;
using AetherRISC.Core.Architecture.Hardware.ISA.Instructions;

namespace AetherRISC.Core.Tests.Architecture.Granular;

public class GranularArithmeticTests
{
    private MachineState _state = new MachineState(SystemConfig.Rv64());
    private InstructionData Data(int rd, int rs1, int rs2, int imm) 
        => new InstructionData { Rd = rd, Rs1 = rs1, Rs2 = rs2, Immediate = (ulong)(long)imm, PC = 0 };

    [Fact]
    public void ADD_Should_Wrap_On_Overflow()
    {
        // Max ulong + 1 should wrap to 0
        _state.Registers.Write(1, ulong.MaxValue); 
        _state.Registers.Write(2, 1);
        
        var inst = new AddInstruction(3, 1, 2);
        inst.Execute(_state, Data(3, 1, 2, 0));
        
        Assert.Equal((ulong)0, _state.Registers.Read(3));
    }

    [Fact]
    public void SUB_Should_Wrap_On_Underflow()
    {
        // 0 - 1 should wrap to Max ulong
        _state.Registers.Write(1, 0); 
        _state.Registers.Write(2, 1);
        
        var inst = new SubInstruction(3, 1, 2);
        inst.Execute(_state, Data(3, 1, 2, 0));
        
        Assert.Equal(ulong.MaxValue, _state.Registers.Read(3));
    }

    [Fact]
    public void ADDI_Negative_Immediate_Is_Sign_Extended()
    {
        // ADDI x1, x0, -1
        // -1 (12-bit) is 0xFFF. In 64-bit 2's comp, it is 0xFFFF...FFFF
        var inst = new AddiInstruction(1, 0, -1);
        inst.Execute(_state, Data(1, 0, 0, -1));
        
        Assert.Equal(ulong.MaxValue, _state.Registers.Read(1));
    }
}



