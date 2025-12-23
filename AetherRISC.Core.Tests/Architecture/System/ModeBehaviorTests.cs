using Xunit;
using AetherRISC.Core.Architecture;
using AetherRISC.Core.Architecture.ISA.Base;

namespace AetherRISC.Core.Tests.Architecture.System;

public class ModeBehaviorTests
{
    private MachineState _state = new MachineState(SystemConfig.Rv64());

    private InstructionData Data(int rd, int rs1, int rs2, int imm) 
        => new InstructionData { Rd = rd, Rs1 = rs1, Rs2 = rs2, Immediate = (ulong)(long)imm, PC = 0 };

    [Fact]
    public void ADDIW_Should_Sign_Extend_32Bit_Result()
    {
        // 64-bit ADD: 0x7FFFFFFF + 1 = 0x80000000 (Positive large number)
        // 32-bit ADD: 0x7FFFFFFF + 1 = 0x80000000 (Negative number in 32-bit 2's comp)
        // ADDIW Should take that negative 32-bit result and sign extend it to 0xFFFFFFFF80000000
        
        _state.Registers.Write(1, 0x7FFFFFFF); // Max 32-bit signed
        var inst = new AddiwInstruction(2, 1, 1);
        inst.Execute(_state, Data(2, 1, 0, 1));
        
        // 0xFFFFFFFF80000000
        ulong expected = 0xFFFFFFFF80000000;
        Assert.Equal(expected, _state.Registers.Read(2));
    }

    [Fact]
    public void ADDW_Should_Truncate_Upper_Bits()
    {
        // x1 = 0x1_0000_0001 (Greater than 32 bits)
        // x2 = 0x1_0000_0001
        // ADDW should ignore the upper '1', add 1+1=2.
        
        _state.Registers.Write(1, 0x100000001);
        _state.Registers.Write(2, 0x100000001);
        
        var inst = new AddwInstruction(3, 1, 2);
        inst.Execute(_state, Data(3, 1, 2, 0));
        
        Assert.Equal((ulong)2, _state.Registers.Read(3));
    }
}
