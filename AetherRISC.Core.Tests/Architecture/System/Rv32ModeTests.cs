using Xunit;
using AetherRISC.Core.Architecture;
using AetherRISC.Core.Architecture.ISA.Base;
using AetherRISC.Core.Architecture.Memory; // SystemBus

namespace AetherRISC.Core.Tests.Architecture.System;

public class Rv32ModeTests
{
    private MachineState _state32;
    private MachineState _state64;

    public Rv32ModeTests()
    {
        _state32 = new MachineState(SystemConfig.Rv32());
        _state32.Memory = new SystemBus(1024);
        
        _state64 = new MachineState(SystemConfig.Rv64());
        _state64.Memory = new SystemBus(1024);
    }

    private InstructionData Data(int rd, int rs1, int rs2, int imm) 
        => new InstructionData { Rd = rd, Rs1 = rs1, Rs2 = rs2, Immediate = (ulong)(long)imm, PC = 0 };

    [Fact]
    public void ADD_Should_Wrap_At_32Bits_In_Rv32()
    {
        // 0xFFFFFFFF + 1
        // RV32: 0x00000000 (Wraps)
        // RV64: 0x100000000 (Does not wrap yet)

        // Setup RV32
        _state32.Registers.Write(1, 0xFFFFFFFF);
        _state32.Registers.Write(2, 1);
        var inst32 = new AddInstruction(3, 1, 2);
        inst32.Execute(_state32, Data(3, 1, 2, 0));

        // Setup RV64
        _state64.Registers.Write(1, 0xFFFFFFFF);
        _state64.Registers.Write(2, 1);
        var inst64 = new AddInstruction(3, 1, 2);
        inst64.Execute(_state64, Data(3, 1, 2, 0));

        Assert.Equal(0u, _state32.Registers.Read(3));
        Assert.Equal(0x100000000u, _state64.Registers.Read(3));
    }

    [Fact]
    public void ADDI_Should_Truncate_Result_In_Rv32()
    {
        // -1 (signed) is 0xFFFFFFFF in 32-bit, 0xFFFFFFFFFFFFFFFF in 64-bit
        // ADDI x1, x0, -1
        
        var inst = new AddiInstruction(1, 0, -1);
        
        // RV32 Test
        inst.Execute(_state32, Data(1, 0, 0, -1));
        Assert.Equal(0xFFFFFFFFu, _state32.Registers.Read(1));
        
        // RV64 Test
        inst.Execute(_state64, Data(1, 0, 0, -1));
        Assert.Equal(0xFFFFFFFFFFFFFFFFu, _state64.Registers.Read(1));
    }

    [Fact]
    public void SLL_Should_Use_5Bit_Shift_In_Rv32()
    {
        // Shift by 32
        // RV64 (mask 0x3F): Shift by 32 (Valid, clears 32-bit val to 0)
        // RV32 (mask 0x1F): 32 & 0x1F = 0. Shift by 0 (Value unchanged)
        
        ulong val = 0xABCD1234;
        
        _state32.Registers.Write(1, val);
        _state32.Registers.Write(2, 32); // Shift Amount
        
        _state64.Registers.Write(1, val);
        _state64.Registers.Write(2, 32); 

        var inst = new SllInstruction(3, 1, 2);

        // RV32: 32 -> 0 shift
        inst.Execute(_state32, Data(3, 1, 2, 0));
        Assert.Equal(val, _state32.Registers.Read(3)); 

        // RV64: 32 -> 32 shift
        inst.Execute(_state64, Data(3, 1, 2, 0));
        // 0xABCD1234 << 32 is 0xABCD123400000000
        Assert.Equal(val << 32, _state64.Registers.Read(3));
    }
}
