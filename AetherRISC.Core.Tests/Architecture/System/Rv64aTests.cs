using Xunit;
using AetherRISC.Core.Architecture;
using AetherRISC.Core.Helpers;
using AetherRISC.Core.Architecture.Memory;

namespace AetherRISC.Core.Tests.Architecture.System;

public class Rv64aTests
{
    private MachineState _state;

    public Rv64aTests()
    {
        _state = new MachineState(SystemConfig.Rv64());
        _state.Memory = new SystemBus(1024);
    }

    private InstructionData Data(int rd, int rs1, int rs2) 
        => new InstructionData { Rd = rd, Rs1 = rs1, Rs2 = rs2 };

    [Fact]
    public void LR_SC_Success_Sequence()
    {
        uint addr = 0x100;
        _state.Memory!.WriteWord(addr, 42);
        _state.Registers.Write(1, addr); 
        _state.Registers.Write(2, 99);   

        var lr = Inst.Lr(3, 1, true); 
        lr.Execute(_state, Data(3, 1, 0));

        Assert.Equal(42u, _state.Registers.Read(3));
        Assert.Equal((ulong)addr, _state.LoadReservationAddress);

        var sc = Inst.Sc(4, 1, 2, true);
        sc.Execute(_state, Data(4, 1, 2));

        Assert.Equal(0u, _state.Registers.Read(4)); 
        Assert.Equal(99u, _state.Memory.ReadWord(addr));
        Assert.Null(_state.LoadReservationAddress); 
    }

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

    [Fact]
    public void LR_SC_Failure_Interference_Simulation()
    {
        uint addr = 0x100;
        _state.Registers.Write(1, addr);
        _state.Registers.Write(2, 99);

        var lr = Inst.Lr(3, 1, true);
        lr.Execute(_state, Data(3, 1, 0));

        _state.LoadReservationAddress = null; 

        var sc = Inst.Sc(4, 1, 2, true);
        sc.Execute(_state, Data(4, 1, 2));

        Assert.Equal(1u, _state.Registers.Read(4)); 
        Assert.NotEqual(99u, _state.Memory!.ReadWord(addr)); 
    }

    [Fact]
    public void AMO_ADD_Atomic_Counter()
    {
        uint addr = 0x100;
        _state.Memory!.WriteWord(addr, 10);
        
        _state.Registers.Write(1, addr);
        _state.Registers.Write(2, 5); 

        var amo = Inst.AmoAdd(3, 1, 2, true); 
        amo.Execute(_state, Data(3, 1, 2));

        Assert.Equal(10u, _state.Registers.Read(3)); 
        Assert.Equal(15u, _state.Memory.ReadWord(addr)); 
    }

    [Fact]
    public void AMO_SWAP_Exchange()
    {
        uint addr = 0x200;
        _state.Memory!.WriteWord(addr, 0xAAAA);
        
        _state.Registers.Write(1, addr);
        _state.Registers.Write(2, 0x5555);

        var amo = Inst.AmoSwap(3, 1, 2, true);
        amo.Execute(_state, Data(3, 1, 2));

        Assert.Equal(0xAAAAu, _state.Registers.Read(3));
        Assert.Equal(0x5555u, _state.Memory.ReadWord(addr));
    }

    [Fact]
    public void AMO_OR_Bitwise_Flag_Set()
    {
        uint addr = 0x300;
        _state.Memory!.WriteWord(addr, 0x1);
        _state.Registers.Write(1, addr);
        _state.Registers.Write(2, 0x2);

        var amo = Inst.AmoOr(3, 1, 2, true);
        amo.Execute(_state, Data(3, 1, 2));

        Assert.Equal(0x3u, _state.Memory.ReadWord(addr));
    }

    [Fact]
    public void AMO_MAX_Signed_Comparison()
    {
        uint addr = 0x400;
        _state.Memory!.WriteWord(addr, unchecked((uint)-5));
        _state.Registers.Write(1, addr);
        _state.Registers.Write(2, 10);

        var amo = Inst.AmoMax(3, 1, 2, true);
        amo.Execute(_state, Data(3, 1, 2));

        Assert.Equal(10u, _state.Memory.ReadWord(addr));
    }
}
