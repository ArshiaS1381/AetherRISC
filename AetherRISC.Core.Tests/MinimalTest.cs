using Xunit;
using AetherRISC.Core.Architecture;
using AetherRISC.Core.Architecture.Memory;
using AetherRISC.Core.Architecture.Pipeline;
using AetherRISC.Core.Hardware.ISA.Base;
using AetherRISC.Core.Hardware.ISA.Encoding;
using AetherRISC.Core.Helpers;
using AetherRISC.Core.Abstractions.Interfaces;
using System.Collections.Generic;

namespace AetherRISC.Core.Tests;

public class MinimalDebugHost : ISystemCallHandler
{
    public List<long> Outputs { get; } = new();
    public void PrintInt(long value) => Outputs.Add(value);
    public void PrintString(string value) { }
    public void Exit(int code) { }
}

public class MinimalTest
{
    [Fact]
    public void Straight_Line_Code_Must_Execute()
    {
        var config = SystemConfig.Rv64();
        var state = new MachineState(config);
        state.Memory = new SystemBus(1024);
        var pipeline = new PipelineController(state);
        var host = new MinimalDebugHost();
        state.Host = host;
        var asm = new TestAssembler();

        // 1. ADDI x10, x0, 123 (a0 = 123)
        asm.Add(pc => Inst.Addi(10, 0, 123));
        
        // 2. ADDI x17, x0, 1   (a7 = 1, PrintInt)
        asm.Add(pc => Inst.Addi(17, 0, 1));
        
        // 3. ECALL (Should print 123)
        asm.Add(pc => Inst.Ecall());

        // 4. ADD x10, x10, x10 (a0 = 246)
        asm.Add(pc => Inst.Add(10, 10, 10));

        var insts = asm.Assemble();
        for(int i=0; i < insts.Count; i++) 
            state.Memory.WriteWord((uint)(i*4), InstructionEncoder.Encode(insts[i]));

        // Run 20 cycles
        for(int i=0; i<20; i++) pipeline.Cycle();

        // Assert Register State
        Assert.Equal((ulong)246, state.Registers.Read(10));
        
        // Assert Output Trace
        Assert.Single(host.Outputs);
        Assert.Equal(123, host.Outputs[0]);
    }
}
