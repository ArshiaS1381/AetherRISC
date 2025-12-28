using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.ISA.Types;
using Xunit;
using AetherRISC.Core.Architecture;
using AetherRISC.Core.Helpers;
using AetherRISC.Core.Architecture.Hardware.Memory;

namespace AetherRISC.Core.Tests.Architecture.System;

public class RvFloatEdgeTests
{
    private MachineState _state;

    public RvFloatEdgeTests()
    {
        _state = new MachineState(SystemConfig.Rv64());
        _state.Memory = new SystemBus(1024);
    }

    private InstructionData Data(int rd, int rs1, int rs2) 
        => new InstructionData { Rd = rd, Rs1 = rs1, Rs2 = rs2 };

    [Fact]
    public void FCVT_S_W_LargeInteger()
    {
        // Convert 1,000,000 to float
        _state.Registers.Write(1, 1000000);
        var inst = Inst.FcvtSW(2, 1);
        inst.Execute(_state, Data(2, 1, 0));
        
        Assert.Equal(1000000.0f, _state.FRegisters.ReadSingle(2));
    }

    [Fact]
    public void FDIV_S_By_Zero_Should_Be_Infinity()
    {
        _state.FRegisters.WriteSingle(1, 10.0f);
        _state.FRegisters.WriteSingle(2, 0.0f);
        
        var inst = Inst.FdivS(3, 1, 2);
        inst.Execute(_state, Data(3, 1, 2));
        
        Assert.True(float.IsPositiveInfinity(_state.FRegisters.ReadSingle(3)));
    }

    [Fact]
    public void FCLASS_S_Positive_Normal()
    {
        _state.FRegisters.WriteSingle(1, 3.14f);
        var inst = new AetherRISC.Core.Architecture.Hardware.ISA.Extensions.F.FclassSInstruction(2, 1);
        inst.Execute(_state, Data(2, 1, 0));
        
        // Bit 6 is "Positive Normal"
        uint res = (uint)_state.Registers.Read(2);
        Assert.Equal(1u << 6, res);
    }

    [Fact]
    public void FMIN_D_Handles_Negative_Zero()
    {
        // -0.0 vs +0.0 -> FMIN should return -0.0
        _state.FRegisters.WriteDouble(1, 0.0);
        _state.FRegisters.WriteDouble(2, -0.0);
        
        var inst = Inst.FminD(3, 1, 2);
        inst.Execute(_state, Data(3, 1, 2));
        
        double res = _state.FRegisters.ReadDouble(3);
        Assert.Equal(-0.0, res);
        // Bit check to ensure it's actually negative zero
        Assert.True(global::System.BitConverter.DoubleToUInt64Bits(res) >> 63 == 1);
    }
}


