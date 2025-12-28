using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.Pipeline.Controller;
using AetherRISC.Core.Architecture.Hardware.Memory;
using AetherRISC.Core.Helpers;
using Xunit;

namespace AetherRISC.Core.Tests.Architecture.Stress;

public class PipelineHazardTests
{
    [Fact]
    public void Load_Use_Hazard_Stall_Sequence()
    {
        var state = new MachineState(SystemConfig.Rv64());
        state.Memory = new SystemBus(1024);
        var pipe = new PipelineController(state);

        state.Registers.Write(2, 0x100);
        state.Memory.WriteWord(0x100, 0xABC);
        
        state.Memory.WriteWord((uint)state.ProgramCounter, 0x00012083); // LW x1, 0(x2)
        state.Memory.WriteWord((uint)state.ProgramCounter + 4, 0x001081B3); // ADD x3, x1, x1

        pipe.Cycle(); 
        pipe.Cycle();
        pipe.Cycle();

        Assert.True(pipe.StallFetch, "Fetch should be stalled due to Load-Use hazard");
        Assert.Equal("NOP", pipe.IdEx.DecodedInst?.Mnemonic);
    }

    [Fact]
    public void Triple_Forwarding_Stress()
    {
        var state = new MachineState(SystemConfig.Rv64());
        state.Memory = new SystemBus(1024);
        var pipe = new PipelineController(state);

        // Sequence:
        // ADDI x1, x0, 10  (x1 = 10)
        // ADDI x1, x1, 5   (x1 = 15 via Forwarding)
        // ADDI x1, x1, 1   (x1 = 16 via Forwarding)
        
        state.Memory.WriteWord((uint)state.ProgramCounter, 0x00A00093);
        state.Memory.WriteWord((uint)state.ProgramCounter + 4, 0x00508093);
        state.Memory.WriteWord((uint)state.ProgramCounter + 8, 0x00108093);

        // 5 stages + 3 instructions = 7-8 cycles to be absolutely safe for Writeback
        for(int i = 0; i < 8; i++) 
        {
            pipe.Cycle();
        }

        Assert.Equal(16ul, state.Registers.Read(1));
    }
}
