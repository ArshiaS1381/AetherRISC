using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.ISA.Types;
using Xunit;
using AetherRISC.Core.Architecture;
using AetherRISC.Core.Architecture.Hardware.Memory; // Needed for SystemBus
using AetherRISC.Core.Architecture.Hardware.ISA.Instructions;

namespace AetherRISC.Core.Tests.Architecture.System;

public class CompleteIsaTests
{
    private MachineState _state;

    public CompleteIsaTests()
    {
        _state = new MachineState(SystemConfig.Rv64());
        // FIX: Initialize Memory so we can write to it!
        _state.Memory = new SystemBus(1024);
    }

    private InstructionData Data(int rd, int rs1, int rs2, int imm) 
        => new InstructionData { Rd = rd, Rs1 = rs1, Rs2 = rs2, Immediate = (ulong)(long)imm, PC = 0 };

    [Fact]
    public void LHU_Zero_Extends_HalfWord()
    {
        // 0xFFFF should become 0x000000000000FFFF (65535) not -1
        _state.Memory!.WriteHalf(100, 0xFFFF);
        _state.Registers.Write(1, 100);
        
        var inst = new LhuInstruction(2, 1, 0);
        inst.Execute(_state, Data(2, 1, 0, 0));
        
        Assert.Equal((ulong)0xFFFF, _state.Registers.Read(2));
    }

    [Fact]
    public void LWU_Zero_Extends_Word()
    {
        // 0xFFFFFFFF should become 0x00000000FFFFFFFF, not 0xFFFFFFFFFFFFFFFF
        _state.Memory!.WriteWord(100, 0xFFFFFFFF);
        _state.Registers.Write(1, 100);
        
        var inst = new LwuInstruction(2, 1, 0);
        inst.Execute(_state, Data(2, 1, 0, 0));
        
        Assert.Equal((ulong)0xFFFFFFFF, _state.Registers.Read(2));
    }

    [Fact]
    public void SLLIW_Shifts_And_Sign_Extends()
    {
        // Start with all 1s (-1)
        // Shift Left 1 bit -> Ends with 0. Lower 32 bits become 0xFFFFFFFE (-2)
        // Result Sign Extended to 64 -> 0xFFFFFFFFFFFFFFFE (-2)
        
        _state.Registers.Write(1, 0xFFFFFFFFFFFFFFFF);
        var inst = new SlliwInstruction(2, 1, 1);
        inst.Execute(_state, Data(2, 1, 0, 1));
        
        Assert.Equal(0xFFFFFFFFFFFFFFFE, _state.Registers.Read(2));
    }

    [Fact]
    public void SRLW_Shifts_Logical_32Bit_Then_Sign_Extends()
    {
        // Value: 0xFF...FF (-1)
        // SRLW 1 bit on 32-bit view:
        // Lower 32 bits: 0xFFFFFFFF -> 0x7FFFFFFF (Sign bit cleared in 32-bit domain)
        // Then Sign Extend 0x7FFFFFFF -> 0x000000007FFFFFFF
        
        _state.Registers.Write(3, 1); // Shift amount
        
        // Setup input in Rs1
        _state.Registers.Write(1, 0xFFFFFFFFFFFFFFFF);

        var inst = new SrlwInstruction(2, 1, 3);
        inst.Execute(_state, Data(2, 1, 3, 0));

        Assert.Equal((ulong)0x7FFFFFFF, _state.Registers.Read(2));
    }
}




