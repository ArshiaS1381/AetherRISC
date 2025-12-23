using Xunit;
using AetherRISC.Core.Architecture;
using AetherRISC.Core.Architecture.ISA.Base;

namespace AetherRISC.Core.Tests.Architecture.Granular;

public class GranularShiftTests
{
    private MachineState _state = new MachineState(SystemConfig.Rv64());
    private InstructionData Data(int rd, int rs1, int rs2, int imm) 
        => new InstructionData { Rd = rd, Rs1 = rs1, Rs2 = rs2, Immediate = (ulong)(long)imm, PC = 0 };

    [Fact]
    public void SLL_Should_Mask_Shift_Amount_To_6_Bits()
    {
        // Shift by 65. 65 = 1000001 binary.
        // 65 & 0x3F = 1.
        // Result should be shifted left by 1, NOT 65 (which would clear register).
        _state.Registers.Write(1, 10);
        _state.Registers.Write(2, 65); 
        
        var inst = new SllInstruction(3, 1, 2);
        inst.Execute(_state, Data(3, 1, 2, 0));
        
        Assert.Equal((ulong)20, _state.Registers.Read(3));
    }

    [Fact]
    public void SRA_Should_Maintain_Sign_Bit()
    {
        // Arithmetic Shift Right on negative number
        // 0xF000...0000 >> 4 should be 0xFF00...0000
        ulong negVal = 0xF000000000000000;
        _state.Registers.Write(1, negVal);
        _state.Registers.Write(2, 4);

        var inst = new SraInstruction(3, 1, 2);
        inst.Execute(_state, Data(3, 1, 2, 0));
        
        ulong expected = 0xFF00000000000000;
        Assert.Equal(expected, _state.Registers.Read(3));
    }

    [Fact]
    public void SRL_Should_Zero_Fill()
    {
        // Logical Shift Right on negative number
        // 0xF000...0000 >> 4 should be 0x0F00...0000
        ulong negVal = 0xF000000000000000;
        _state.Registers.Write(1, negVal);
        _state.Registers.Write(2, 4);

        var inst = new SrlInstruction(3, 1, 2);
        inst.Execute(_state, Data(3, 1, 2, 0));
        
        ulong expected = 0x0F00000000000000;
        Assert.Equal(expected, _state.Registers.Read(3));
    }
}
