using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.ISA.Types;
using Xunit;
using AetherRISC.Core.Helpers;
using AetherRISC.Core.Architecture.Hardware.Memory;

namespace AetherRISC.Core.Tests.Architecture.System;

public class Rv64aTests
{
    private MachineState _state;
    public Rv64aTests() { _state = new MachineState(SystemConfig.Rv64()); _state.Memory = new SystemBus(1024); }
    private InstructionData Data(int rd, int rs1, int rs2) => new InstructionData { Rd = rd, Rs1 = rs1, Rs2 = rs2 };

    [Fact]
    public void LR_SC_Failure_Address_Mismatch()
    {
        _state.Registers.Write(1, 0x100); 
        _state.Registers.Write(2, 0x200); 
        _state.Registers.Write(3, 99);    

        var lr = Inst.Lr(0, 1, true);
        lr.Execute(_state, Data(0, 1, 0));

        var sc = Inst.Sc(4, 2, 3, true);
        sc.Execute(_state, Data(4, 2, 3));

        Assert.Equal(1u, _state.Registers.Read(4)); 
        Assert.Null(_state.LoadReservationAddress); 
    }
}
