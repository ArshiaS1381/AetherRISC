using Xunit;
using AetherRISC.Core.Architecture;
using AetherRISC.Core.Architecture.Memory;
using AetherRISC.Core.Architecture.Pipeline;
using AetherRISC.Core.Hardware.ISA.Base;
using AetherRISC.Core.Hardware.ISA.Encoding;
using AetherRISC.Core.Helpers;
using System.Collections.Generic;

namespace AetherRISC.Core.Tests;

public class MulTest
{
    [Fact]
    public void Mul_Should_Calculate_Correctly()
    {
        var config = SystemConfig.Rv64();
        var state = new MachineState(config);
        state.Memory = new SystemBus(1024);
        var pipeline = new PipelineController(state);
        var asm = new TestAssembler();

        // 1. Setup: x1 = 5, x2 = 10
        state.Registers.Write(1, 5);
        state.Registers.Write(2, 10);

        // 2. MUL x3, x1, x2 -> 5 * 10 = 50
        asm.Add(pc => Inst.Mul(3, 1, 2));

        var insts = asm.Assemble();
        for(int i=0; i < insts.Count; i++) 
            state.Memory.WriteWord((uint)(i*4), InstructionEncoder.Encode(insts[i]));

        // Run 5 cycles (Fetch -> WB)
        for(int i=0; i<5; i++) pipeline.Cycle();

        Assert.Equal((ulong)50, state.Registers.Read(3));
    }

    [Fact]
    public void Mul_Should_Use_Forwarding()
    {
        // Test MUL immediately after an ADDI that defines one of its operands
        var config = SystemConfig.Rv64();
        var state = new MachineState(config);
        state.Memory = new SystemBus(1024);
        var pipeline = new PipelineController(state);
        var asm = new TestAssembler();

        // 1. ADDI x1, x0, 6
        asm.Add(pc => Inst.Addi(1, 0, 6));
        
        // 2. MUL x2, x1, x1 -> Should use forwarded x1 (6) * 6 = 36
        // If forwarding fails, it might read x1 as 0 -> result 0
        asm.Add(pc => Inst.Mul(2, 1, 1));

        var insts = asm.Assemble();
        for(int i=0; i < insts.Count; i++) 
            state.Memory.WriteWord((uint)(i*4), InstructionEncoder.Encode(insts[i]));

        for(int i=0; i<10; i++) pipeline.Cycle();

        Assert.Equal((ulong)36, state.Registers.Read(2));
    }
}
