using Xunit;
using AetherRISC.Core.Architecture;
using AetherRISC.Core.Architecture.Memory;
using AetherRISC.Core.Architecture.Pipeline;
using AetherRISC.Core.Architecture.ISA.Encoding;
using AetherRISC.Core.Helpers;
using System.Collections.Generic;

namespace AetherRISC.Core.Tests.Architecture.System;

public class PipelineHazardTests
{
    private MachineState _state;
    private PipelineController _pipeline;
    private TestAssembler _asm;

    public PipelineHazardTests() {
        _state = new MachineState(SystemConfig.Rv64());
        _state.Memory = new SystemBus(1024);
        _pipeline = new PipelineController(_state);
        _asm = new TestAssembler();
    }

    [Fact]
    public void RAW_Hazard_Adjacent_Instructions()
    {
        _asm.Add(pc => Inst.Addi(1, 0, 10));
        _asm.Add(pc => Inst.Addi(2, 1, 5)); 

        Run();

        Assert.Equal((ulong)10, _state.Registers.Read(1));
        Assert.Equal((ulong)15, _state.Registers.Read(2));
    }

    [Fact]
    public void RAW_Hazard_Load_Use()
    {
        // Ensure memory exists (bang operator ! since we initialized it in ctor)
        _state.Memory!.WriteWord(100, 42);

        _asm.Add(pc => Inst.Lw(1, 0, 100));
        _asm.Add(pc => Inst.Addi(2, 1, 5));

        Run();

        Assert.Equal((ulong)42, _state.Registers.Read(1));
        Assert.Equal((ulong)47, _state.Registers.Read(2));
    }

    private void Run() {
        var insts = _asm.Assemble();
        // Null check for safety, though ctor ensures it
        if (_state.Memory == null) return;

        for(int i=0; i < insts.Count; i++) 
            _state.Memory.WriteWord((uint)(i*4), InstructionEncoder.Encode(insts[i]));
        
        for(int i=0; i<10; i++) _pipeline.Cycle();
    }
}
