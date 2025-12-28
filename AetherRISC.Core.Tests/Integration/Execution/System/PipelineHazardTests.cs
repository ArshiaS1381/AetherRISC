using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.Pipeline.Controller;
using Xunit;
using AetherRISC.Core.Architecture;
using AetherRISC.Core.Architecture.Hardware.Memory;
using AetherRISC.Core.Architecture.Hardware.Pipeline;
using AetherRISC.Core.Architecture.Hardware.ISA.Encoding;
using AetherRISC.Core.Architecture.Hardware.ISA.Decoding;
using AetherRISC.Core.Architecture.Hardware.ISA.Instructions;
using System;

namespace AetherRISC.Core.Tests.System;

public class PipelineHazardTests
{
    private MachineState Setup()
    {
        var _ = new InstructionDecoder(); 
        var state = new MachineState(SystemConfig.Rv64());
        state.Memory = new SystemBus(1024);
        state.ProgramCounter = 0; return state;
    }

    [Fact]
    public void Debug_Register_Write_Timing()
    {
        // Test 1: Verify Cycle-Accurate Writeback Timing
        var state = Setup();
        var pipeline = new PipelineController(state);

        // 0: ADDI x1, x0, 0xAA (170)
        state.Memory!.WriteWord(0, InstructionEncoder.Encode(new AddiInstruction(1, 0, 0xAA)));

        // Cycle 1: Fetch
        pipeline.Cycle(); 
        Assert.Equal((ulong)0, state.Registers.Read(1));
        
        // Cycle 2: Decode
        pipeline.Cycle(); 
        Assert.Equal((ulong)0, state.Registers.Read(1));

        // Cycle 3: Execute
        pipeline.Cycle(); 
        // ACCURACY CHECK: Result is calculated, but NOT committed to Register File yet.
        Assert.Equal((ulong)0, state.Registers.Read(1)); 

        // Cycle 4: Memory
        pipeline.Cycle();
        // ACCURACY CHECK: Still in pipeline latches (ExMem -> MemWb).
        Assert.Equal((ulong)0, state.Registers.Read(1));

        // Cycle 5: Writeback
        pipeline.Cycle(); 
        // ACCURACY CHECK: NOW it must be committed.
        Assert.Equal((ulong)0xAA, state.Registers.Read(1));
    }

    [Fact]
    public void Hazard_Read_After_Write_With_Forwarding()
    {
        // Test 2: Prove Forwarding works even though Writeback is delayed
        var state = Setup();
        var pipeline = new PipelineController(state);

        // 0: ADDI x1, x0, 10
        // 4: ADD  x2, x1, x0  (Reads x1 immediately)
        state.Memory!.WriteWord(0, InstructionEncoder.Encode(new AddiInstruction(1, 0, 10)));
        state.Memory!.WriteWord(4, InstructionEncoder.Encode(new AddInstruction(2, 1, 0)));

        // Run enough cycles for everything to flush (10 cycles)
        for(int i=0; i<10; i++) pipeline.Cycle();

        // Verification:
        // x1 should be 10 (Written at Cycle 5)
        // x2 should be 10. 
        // If x2 is 0, Forwarding failed (it read the stale x1 value from RegFile).
        Assert.Equal((ulong)10, state.Registers.Read(1));
        
        if (state.Registers.Read(2) == 0)
        {
            throw new global::System.Exception("Forwarding Failed: x2 read 0 (stale) instead of 10 (forwarded).");
        }
        Assert.Equal((ulong)10, state.Registers.Read(2));
    }

    [Fact]
    public void Debug_Fibonacci_Sequence()
    {
        // Test 3: Full sequence check
        var state = Setup();
        var pipeline = new PipelineController(state);

        // 0: ADDI x1, x0, 0
        state.Memory!.WriteWord(0x00, InstructionEncoder.Encode(new AddiInstruction(1, 0, 0)));
        // 4: ADDI x2, x0, 1
        state.Memory!.WriteWord(0x04, InstructionEncoder.Encode(new AddiInstruction(2, 0, 1)));
        // 8: ADD x4, x1, x2 (x4 = 0+1 = 1)
        state.Memory!.WriteWord(0x08, InstructionEncoder.Encode(new AddInstruction(4, 1, 2)));
        // C: ADDI x1, x2, 0 (x1 = 1)
        state.Memory!.WriteWord(0x0C, InstructionEncoder.Encode(new AddiInstruction(1, 2, 0)));
        // 10: ADDI x2, x4, 0 (x2 = 1)
        state.Memory!.WriteWord(0x10, InstructionEncoder.Encode(new AddiInstruction(2, 4, 0)));

        // Run 15 cycles to clear pipeline
        for(int i=0; i<15; i++) pipeline.Cycle();

        Assert.Equal((ulong)1, state.Registers.Read(4)); // First Sum
        Assert.Equal((ulong)1, state.Registers.Read(1)); // Shifted
        Assert.Equal((ulong)1, state.Registers.Read(2)); // Shifted
    }
}




