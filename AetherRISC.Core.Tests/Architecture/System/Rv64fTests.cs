using Xunit;
using AetherRISC.Core.Architecture;
using AetherRISC.Core.Helpers;
using AetherRISC.Core.Architecture.Memory;

namespace AetherRISC.Core.Tests.Architecture.System;

public class Rv64fTests
{
    private MachineState _state;

    public Rv64fTests()
    {
        _state = new MachineState(SystemConfig.Rv64());
        _state.Memory = new SystemBus(1024);
    }

    // FIX: Added 'imm' parameter to helper
    private InstructionData Data(int rd, int rs1, int rs2, int imm = 0) 
        => new InstructionData { Rd = rd, Rs1 = rs1, Rs2 = rs2, Immediate = (ulong)(long)imm };

    [Fact]
    public void FLW_FSW_RoundTrip()
    {
        float pi = 3.14159f;
        
        // Use global::System to avoid namespace collision
        uint piBits = global::System.BitConverter.SingleToUInt32Bits(pi);
        
        _state.Memory!.WriteWord(0x100, piBits);
        _state.Registers.Write(1, 0x100); // x1 = 0x100

        // 1. FLW f1, 0(x1)
        var flw = Inst.Flw(1, 1, 0); 
        flw.Execute(_state, Data(1, 1, 0, 0)); // imm=0

        // Check NaN Boxing (Upper 32 bits should be 1s)
        ulong val = _state.FRegisters.Read(1);
        Assert.Equal(0xFFFFFFFF00000000 | (ulong)piBits, val);
        Assert.Equal(pi, _state.FRegisters.ReadSingle(1));

        // 2. FSW f1, 4(x1) -> Should write to 0x104
        var fsw = Inst.Fsw(1, 1, 4); 
        // FIX: Pass '4' as immediate to the Execute data
        fsw.Execute(_state, Data(0, 1, 1, 4)); 

        // Verify Memory at offset location
        Assert.Equal(piBits, _state.Memory.ReadWord(0x104));
    }
}
