using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.ISA.Types;
using Xunit;
using AetherRISC.Core.Helpers;
using AetherRISC.Core.Architecture.Hardware.Memory;

namespace AetherRISC.Core.Tests.Architecture.System;

public class Rv32dTests
{
    private readonly MachineState _state;

    public Rv32dTests()
    {
        // FORCE 32-BIT MODE
        _state = new MachineState(SystemConfig.Rv32());
        _state.Memory = new SystemBus(1024);
    }

    private static InstructionData Data(int rd, int rs1, int rs2, int imm = 0) =>
        new InstructionData
        {
            Rd = rd,
            Rs1 = rs1,
            Rs2 = rs2,
            Imm = imm,
            Immediate = (ulong)(long)imm,
            PC = 0
        };

    [Fact]
    public void FLD_FSD_Should_Work_In_32Bit_Mode()
    {
        // 1. Setup
        double pi = 3.14159265359;
        ulong piBits = global::System.BitConverter.DoubleToUInt64Bits(pi);

        uint addr = 0x100;
        _state.Memory!.WriteDouble(addr, piBits);
        _state.Registers.Write(1, addr); // x1 holds address

        // 2. FLD f1, 0(x1)
        var fld = Inst.Fld(1, 1, 0);
        fld.Execute(_state, Data(1, 1, 0, 0));

        Assert.Equal(pi, _state.FRegisters.ReadDouble(1));

        // 3. FSD f1, 8(x1) -> Write to 0x108
        var fsd = Inst.Fsd(1, 1, 8);
        fsd.Execute(_state, Data(0, 1, 1, 8));

        ulong memBack = _state.Memory.ReadDouble(0x108);
        Assert.Equal(piBits, memBack);
    }
}
