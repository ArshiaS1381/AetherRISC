using Xunit;
using AetherRISC.Core.Architecture;
using AetherRISC.Core.Architecture.Memory;
using AetherRISC.Core.Architecture.Pipeline;
using AetherRISC.Core.Hardware.ISA.Base;
using AetherRISC.Core.Hardware.ISA.Encoding;
using AetherRISC.Core.Helpers;
using System.Collections.Generic;

namespace AetherRISC.Core.Tests;

public class HazardTests
{
    [Fact]
    public void Load_Use_Hazard_Should_Forward_Data()
    {
        var config = SystemConfig.Rv64();
        var state = new MachineState(config);
        // Ensure SystemBus/MemoryController usage is consistent
        state.Memory = new SystemBus(4096); 
        var pipeline = new PipelineController(state);
        var asm = new TestAssembler();

        // 1. Setup Memory: Write 42 at address 100
        state.Memory.WriteDoubleWord(100, 42);

        // 2. Setup Registers: x1 = 100
        state.Registers.Write(1, 100);

        // 3. Code Sequence
        // LD x2, 0(x1)  -> Loads 42 into x2
        asm.Add(pc => Inst.Ld(2, 1, 0));
        
        // ADD x3, x2, x1 -> Uses x2 IMMEDIATELY. Should be 42 + 100 = 142.
        // If forwarding fails, x2 will be read as 0 (old value), result 100.
        asm.Add(pc => Inst.Add(3, 2, 1));

        var insts = asm.Assemble();
        for(int i=0; i < insts.Count; i++) 
            state.Memory.WriteWord((uint)(i*4), InstructionEncoder.Encode(insts[i]));

        // Run 10 cycles
        for(int i=0; i<10; i++) pipeline.Cycle();

        // Assert
        Assert.Equal((ulong)42, state.Registers.Read(2)); // Load worked
        Assert.Equal((ulong)142, state.Registers.Read(3)); // Forwarding worked
    }
}
