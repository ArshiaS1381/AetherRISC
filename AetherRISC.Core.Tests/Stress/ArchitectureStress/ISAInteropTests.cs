using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.ISA.Types;
using AetherRISC.Core.Helpers;
using Xunit;
using System;

namespace AetherRISC.Core.Tests.Architecture.Stress;

public class ISAInteropTests
{
    [Fact]
    public void FCVT_Boundary_Condition_Stress()
    {
        var state = new MachineState(SystemConfig.Rv64());
        var data = new InstructionData { Rd = 10, Rs1 = 1 };

        state.FRegisters.WriteSingle(1, float.MaxValue);
        
        // Fix: Use the generated helper which likely doesn't include rm (rounding mode) 
        // as a parameter if it's not in the main public constructor.
        var inst = Inst.FcvtWS(10, 1); 
        inst.Execute(state, data);

        Assert.Equal(0x7FFFFFFFu, state.Registers.Read(10));
    }

    [Fact]
    public void NanBoxed_Double_In_Single_Precision_Op()
    {
        var state = new MachineState(SystemConfig.Rv64());
        ulong nanBoxed = 0xFFFFFFFF00000000 | (ulong)BitConverter.SingleToInt32Bits(1.5f);
        state.Registers.Write(1, nanBoxed);
        
        var fmv = Inst.FmvWX(1, 1);
        fmv.Execute(state, new InstructionData { Rd = 1, Rs1 = 1 });
        
        Assert.Equal(1.5f, state.FRegisters.ReadSingle(1));
    }
}
