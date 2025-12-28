using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.ISA.Types;
using Xunit;
using AetherRISC.Core.Architecture;
using AetherRISC.Core.Architecture.Hardware.ISA.Instructions;

namespace AetherRISC.Core.Tests.Architecture.Granular;

public class GranularRegisterTests
{
    private MachineState _state = new MachineState(SystemConfig.Rv64());
    private InstructionData Data(int rd, int rs1, int rs2, int imm) 
        => new InstructionData { Rd = rd, Rs1 = rs1, Rs2 = rs2, Immediate = (ulong)(long)imm, PC = 0 };

    [Fact]
    public void X0_Must_Always_Be_Zero_Even_After_Direct_Write()
    {
        // Try writing via MachineState backdoor
        _state.Registers.Write(0, 0xDEADBEEF);
        Assert.Equal((ulong)0, _state.Registers.Read(0));
    }

    [Fact]
    public void ADDI_To_X0_Must_Discard_Result()
    {
        // ADDI x0, x0, 100
        var inst = new AddiInstruction(0, 0, 100);
        inst.Execute(_state, Data(0, 0, 0, 100));
        
        Assert.Equal((ulong)0, _state.Registers.Read(0));
    }

    [Fact]
    public void Registers_Must_Be_Independent()
    {
        _state.Registers.Write(1, 0xAA);
        _state.Registers.Write(2, 0xBB);
        
        Assert.Equal((ulong)0xAA, _state.Registers.Read(1));
        Assert.Equal((ulong)0xBB, _state.Registers.Read(2));
    }
}



