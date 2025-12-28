using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.Pipeline.Controller;
using Xunit;
using AetherRISC.Core.Architecture;
using AetherRISC.Core.Architecture.Hardware.Memory;
using AetherRISC.Core.Architecture.Hardware.Pipeline;
using AetherRISC.Core.Architecture.Hardware.ISA.Encoding;
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
        _state.ProgramCounter = 0; // FIX: Initialize PC to 0 where code is loaded
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
        _state.Memory!.WriteWord(100, 42);

        _asm.Add(pc => Inst.Lw(1, 0, 100));
        _asm.Add(pc => Inst.Addi(2, 1, 5));

        Run();

        Assert.Equal((ulong)42, _state.Registers.Read(1));
        Assert.Equal((ulong)47, _state.Registers.Read(2));
    }

    private void Run() {
        var insts = _asm.Assemble();
        if (_state.Memory == null) return;

        for(int i=0; i < insts.Count; i++) 
            _state.Memory.WriteWord((uint)(i*4), InstructionEncoder.Encode(insts[i]));
        
        for(int i=0; i<10; i++) _pipeline.Cycle();
    }
}
