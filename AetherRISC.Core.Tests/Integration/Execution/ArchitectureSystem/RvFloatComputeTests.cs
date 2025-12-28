using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.ISA.Types;
using Xunit;
using AetherRISC.Core.Architecture;
using AetherRISC.Core.Helpers;
using AetherRISC.Core.Architecture.Hardware.Memory;

namespace AetherRISC.Core.Tests.Architecture.System;

public class RvFloatComputeTests
{
    private MachineState _state;

    public RvFloatComputeTests()
    {
        _state = new MachineState(SystemConfig.Rv64());
        _state.Memory = new SystemBus(1024);
    }
    
    void SetF(int reg, float val) => _state.FRegisters.WriteSingle(reg, val);
    void SetD(int reg, double val) => _state.FRegisters.WriteDouble(reg, val);

    private InstructionData Data(int rd, int rs1, int rs2) 
        => new InstructionData { Rd = rd, Rs1 = rs1, Rs2 = rs2 };

    [Fact]
    public void FADD_S_Should_Add_Floats()
    {
        SetF(1, 2.5f);
        SetF(2, 3.5f);
        var inst = Inst.FaddS(3, 1, 2);
        inst.Execute(_state, Data(3, 1, 2));
        Assert.Equal(6.0f, _state.FRegisters.ReadSingle(3));
    }

    [Fact]
    public void FADD_D_Should_Add_Doubles()
    {
        SetD(1, 123.456);
        SetD(2, 789.123);
        var inst = Inst.FaddD(3, 1, 2);
        inst.Execute(_state, Data(3, 1, 2));
        
        // FIX: Use 10 decimal places of precision instead of strict equality
        Assert.Equal(912.579, _state.FRegisters.ReadDouble(3), 10);
    }

    [Fact]
    public void FSGNJ_S_Should_Copy_Sign()
    {
        SetF(1, 2.0f);
        SetF(2, -5.0f); 
        var inst = Inst.FsgnjS(3, 1, 2);
        inst.Execute(_state, Data(3, 1, 2));
        Assert.Equal(-2.0f, _state.FRegisters.ReadSingle(3));
    }

    [Fact]
    public void FCVT_D_S_Should_Convert_Precision()
    {
        float start = 3.14f;
        SetF(1, start);
        var inst = Inst.FcvtDS(2, 1);
        inst.Execute(_state, Data(2, 1, 0));
        
        double res = _state.FRegisters.ReadDouble(2);
        Assert.Equal((double)start, res, 6); // Approx check
    }

    [Fact]
    public void FEQ_S_Should_Compare()
    {
        SetF(1, 10.0f);
        SetF(2, 10.0f);
        var inst = Inst.FeqS(3, 1, 2);
        inst.Execute(_state, Data(3, 1, 2));
        Assert.Equal(1u, _state.Registers.Read(3));
    }
}


