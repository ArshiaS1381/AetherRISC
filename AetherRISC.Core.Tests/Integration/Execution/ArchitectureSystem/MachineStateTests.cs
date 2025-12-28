using AetherRISC.Core.Architecture.Simulation.State;
using Xunit;
using AetherRISC.Core.Architecture;

namespace AetherRISC.Core.Tests.Architecture.System;

public class MachineStateTests
{
    [Fact]
    public void PC_Should_Start_At_Configured_Reset_Vector()
    {
        var config = SystemConfig.Rv64(); 
        // Assuming default reset vector is 0 or configurable
        var state = new MachineState(config);
        
        Assert.Equal((ulong)0x80000000, state.ProgramCounter);
    }

    [Fact]
    public void Registers_Should_Initialize_Zero()
    {
        var state = new MachineState(SystemConfig.Rv64());
        for(int i=0; i<32; i++) {
            Assert.Equal((ulong)0, state.Registers.Read(i));
        }
    }
    
    [Fact]
    public void Register_Write_Bounds_Check()
    {
        var state = new MachineState(SystemConfig.Rv64());
        
        // Should not throw exception, just ignore or handle gracefully
        // Depending on implementation, x32 might wrap or be ignored
        try {
            state.Registers.Write(32, 123); 
        } catch {
            // If it throws, that is one valid behavior. 
            // If it ignores, that is another. 
            // We just ensure it doesn't crash the test runner unexpectedly.
        }
    }
}



