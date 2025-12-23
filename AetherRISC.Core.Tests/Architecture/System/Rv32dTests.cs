using Xunit;
using AetherRISC.Core.Architecture;
using AetherRISC.Core.Helpers;
using AetherRISC.Core.Architecture.Memory;

namespace AetherRISC.Core.Tests.Architecture.System;

public class Rv32dTests
{
    private MachineState _state;

    public Rv32dTests()
    {
        // FORCE 32-BIT MODE
        _state = new MachineState(SystemConfig.Rv32());
        _state.Memory = new SystemBus(1024);
    }

    private InstructionData Data(int rd, int rs1, int rs2, int imm=0) 
        => new InstructionData { Rd = rd, Rs1 = rs1, Rs2 = rs2, Immediate = (ulong)(long)imm };

    [Fact]
    public void FLD_FSD_Should_Work_In_32Bit_Mode()
    {
        // 1. Setup
        double pi = 3.14159265359;
        ulong piBits = global::System.BitConverter.DoubleToUInt64Bits(pi);
        
        uint addr = 0x100;
        _state.Memory!.WriteDouble(addr, piBits);
        _state.Registers.Write(1, addr); // x1 (32-bit reg) holds address

        // 2. FLD f1, 0(x1)
        // Even though machine is 32-bit, f-registers are 64-bit
        var fld = Inst.Fld(1, 1, 0); 
        fld.Execute(_state, Data(1, 1, 0, 0));

        // Verify Register
        Assert.Equal(pi, _state.FRegisters.ReadDouble(1));

        // 3. FSD f1, 8(x1) -> Write to 0x108
        var fsd = Inst.Fsd(1, 1, 8);
        fsd.Execute(_state, Data(0, 1, 1, 8)); // Source in Rs2 slot for Store

        // Verify Memory
        ulong memBack = _state.Memory.ReadDouble(0x108);
        Assert.Equal(piBits, memBack);
    }
}
