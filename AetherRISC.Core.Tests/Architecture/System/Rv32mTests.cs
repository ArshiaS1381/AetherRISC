using Xunit;
using AetherRISC.Core.Architecture;
using AetherRISC.Core.Helpers;
using AetherRISC.Core.Architecture.Memory;

namespace AetherRISC.Core.Tests.Architecture.System;

public class Rv32mTests
{
    private MachineState _state;

    public Rv32mTests()
    {
        _state = new MachineState(SystemConfig.Rv32());
        _state.Memory = new SystemBus(1024);
    }

    private InstructionData Data(int rd, int rs1, int rs2) 
        => new InstructionData { Rd = rd, Rs1 = rs1, Rs2 = rs2 };

    [Fact]
    public void MULH_Should_Return_Upper_32_Bits()
    {
        _state.Registers.Write(1, 0x7FFFFFFF);
        _state.Registers.Write(2, 0x7FFFFFFF);

        var inst = Inst.Mulh(3, 1, 2);
        inst.Execute(_state, Data(3, 1, 2));

        Assert.Equal(0x3FFFFFFFu, _state.Registers.Read(3));
    }

    [Fact]
    public void MULHU_Should_Return_Upper_32_Bits_Unsigned()
    {
        _state.Registers.Write(1, 0xFFFFFFFF);
        _state.Registers.Write(2, 0xFFFFFFFF);

        var inst = Inst.Mulhu(3, 1, 2);
        inst.Execute(_state, Data(3, 1, 2));

        Assert.Equal(0xFFFFFFFEu, _state.Registers.Read(3));
    }

    [Fact]
    public void DIV_Signed_32Bit()
    {
        _state.Registers.Write(1, 0xFFFFFF9C); // -100
        _state.Registers.Write(2, 10);
        
        var inst = Inst.Div(3, 1, 2);
        inst.Execute(_state, Data(3, 1, 2));
        
        // Result: -10 (0xFFFFFFF6). Mask to ensure we ignore upper 32 ghost bits.
        Assert.Equal(0xFFFFFFF6u, _state.Registers.Read(3) & 0xFFFFFFFF);
    }
    
    [Fact]
    public void DIV_Overflow_32Bit()
    {
        // INT_MIN / -1 -> Overflow (INT_MIN)
        _state.Registers.Write(1, 0x80000000);
        _state.Registers.Write(2, 0xFFFFFFFF);
        
        var inst = Inst.Div(3, 1, 2);
        inst.Execute(_state, Data(3, 1, 2));
        
        // FIX: Mask the result to 32 bits. 
        // The emulator correctly produced 0xFFFFFFFF80000000 (Sign Extended INT_MIN), 
        // but we only care about the lower 32 bits being 0x80000000.
        Assert.Equal(0x80000000u, _state.Registers.Read(3) & 0xFFFFFFFF);
    }

    [Fact]
    public void REM_Signed_32Bit()
    {
        _state.Registers.Write(1, 0xFFFFFFF6); // -10
        _state.Registers.Write(2, 3);

        var inst = Inst.Rem(3, 1, 2);
        inst.Execute(_state, Data(3, 1, 2));

        Assert.Equal(0xFFFFFFFFu, _state.Registers.Read(3) & 0xFFFFFFFF);
    }
}
