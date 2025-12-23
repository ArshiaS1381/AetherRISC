using Xunit;
using AetherRISC.Core.Architecture;
using AetherRISC.Core.Helpers;
using AetherRISC.Core.Architecture.Memory;

namespace AetherRISC.Core.Tests.Architecture.System;

public class Rv64mTests
{
    private MachineState _state;

    public Rv64mTests()
    {
        _state = new MachineState(SystemConfig.Rv64());
        _state.Memory = new SystemBus(1024);
    }

    private InstructionData Data(int rd, int rs1, int rs2) 
        => new InstructionData { Rd = rd, Rs1 = rs1, Rs2 = rs2 };

    [Fact]
    public void MULH_Signed_High_Product()
    {
        // -1 * -1 = 1. High part should be 0.
        // But let's try huge numbers.
        // MaxValue * 2. 
        // 0x7FFFFFFFFFFFFFFF * 2 = 0xFFFFFFFFFFFFFFFE (fits in 64 bits unsigned, but signed overflow)
        
        // Let's use simple logic: (2^62) * 4 = 2^64.
        // Rs1 = 1 << 62
        // Rs2 = 4
        // Result = 1 << 64. 
        // Low part (MUL) = 0. 
        // High part (MULH) = 1.
        
        long val1 = 1L << 62; 
        long val2 = 4;
        
        _state.Registers.Write(1, (ulong)val1);
        _state.Registers.Write(2, (ulong)val2);

        var inst = Inst.Mulh(3, 1, 2);
        inst.Execute(_state, Data(3, 1, 2));

        Assert.Equal(1u, _state.Registers.Read(3));
    }

    [Fact]
    public void MULHU_Unsigned_High_Product()
    {
        // MaxValue * MaxValue (Unsigned)
        // 0xFFFFFFFFFFFFFFFF * 0xFFFFFFFFFFFFFFFF
        // This is (2^64 - 1)^2 = 2^128 - 2^65 + 1
        // High part should be 0xFFFFFFFFFFFFFFFE (Max - 1)
        
        _state.Registers.Write(1, ulong.MaxValue);
        _state.Registers.Write(2, ulong.MaxValue);

        var inst = Inst.Mulhu(3, 1, 2);
        inst.Execute(_state, Data(3, 1, 2));

        Assert.Equal(ulong.MaxValue - 1, _state.Registers.Read(3));
    }

    [Fact]
    public void DIVU_Should_Treat_Negative_As_Large_Positive()
    {
        // DIV (Signed): -1 / 2 = 0
        // DIVU (Unsigned): (2^64 - 1) / 2 = 2^63 - 1 (Huge number)
        
        _state.Registers.Write(1, ulong.MaxValue); // -1 signed, Huge unsigned
        _state.Registers.Write(2, 2);

        var inst = Inst.Divu(3, 1, 2);
        inst.Execute(_state, Data(3, 1, 2));

        ulong expected = ulong.MaxValue / 2;
        Assert.Equal(expected, _state.Registers.Read(3));
    }

    [Fact]
    public void MULW_Should_Truncate_Then_SignExtend()
    {
        // 0x100000001 * 1
        // Lower 32 bits are 1 * 1 = 1.
        // Result should be 1.
        
        // Let's try something that overflows 32 bits but not 64.
        // Rs1 = 0xFFFFFFFF (32-bit -1)
        // Rs2 = 0xFFFFFFFF (32-bit -1)
        // MULW: (-1 * -1) in 32-bit = 1.
        
        // Wait, input registers are taken as is. 
        // MULW multiplies the lower 32 bits of source registers.
        // 0xFF...FF is -1.
        // -1 * -1 = 1.
        // Result is 1.
        
        _state.Registers.Write(1, 0xFFFFFFFFFFFFFFFF);
        _state.Registers.Write(2, 0xFFFFFFFFFFFFFFFF);
        
        var inst = Inst.Mulw(3, 1, 2);
        inst.Execute(_state, Data(3, 1, 2));
        
        Assert.Equal(1u, _state.Registers.Read(3));
    }

    [Fact]
    public void DIVW_Should_Sign_Extend_Result()
    {
        // Divide -100 by 10 using 32-bit math.
        // Result: -10.
        // In 64-bit register, -10 must be sign-extended (0xFF...F6).
        
        _state.Registers.Write(1, unchecked((ulong)-100));
        _state.Registers.Write(2, 10);
        
        var inst = Inst.Divw(3, 1, 2);
        inst.Execute(_state, Data(3, 1, 2));
        
        Assert.Equal(unchecked((ulong)-10L), _state.Registers.Read(3));
    }

    [Fact]
    public void DIVUW_Should_Zero_Extend_Input_Then_Sign_Extend_Result()
    {
        // This is tricky. 
        // DIVUW takes lower 32 bits of RS1 and RS2. Interprets them as Unsigned.
        // Result is sign-extended to 64 bits.
        
        // Case: RS1 = 0xF...F (All ones). Lower 32 is 0xFFFFFFFF.
        // As unsigned 32-bit: 4,294,967,295.
        // Divide by 2.
        // Result = 2,147,483,647 (0x7FFFFFFF).
        // This is a positive integer. Sign extended -> 0x000000007FFFFFFF.
        
        _state.Registers.Write(1, ulong.MaxValue);
        _state.Registers.Write(2, 2);
        
        var inst = Inst.Divuw(3, 1, 2);
        inst.Execute(_state, Data(3, 1, 2));
        
        Assert.Equal(0x7FFFFFFFu, _state.Registers.Read(3));
    }
}
