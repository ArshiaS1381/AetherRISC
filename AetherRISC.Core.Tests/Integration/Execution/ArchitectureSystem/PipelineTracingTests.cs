using AetherRISC.Core.Architecture.Hardware.Pipeline.Controller;
using Xunit;
using System;
using AetherRISC.Core.Architecture;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.Memory;
using AetherRISC.Core.Architecture.Hardware.Pipeline;
using AetherRISC.Core.Architecture.Hardware.ISA.Instructions;
using AetherRISC.Core.Architecture.Hardware.ISA.Encoding;
using AetherRISC.Core.Architecture.Hardware.ISA.Decoding;
using AetherRISC.Core.Abstractions.Interfaces;

namespace AetherRISC.Core.Tests.System;

public class PipelineTracingTests
{
    private readonly IInstruction _nop = new NopInstruction();

    [Fact]
    public void Trace_Ebreak_Propagation_Step_By_Step()
    {
        // 1. Setup
        var _ = new InstructionDecoder();
        var state = new MachineState(SystemConfig.Rv64());
        state.Memory = new SystemBus(1024);
        state.ProgramCounter = 0; // FIX: Initialize PC to 0 where code is loaded
        var pipeline = new PipelineController(state);

        // 2. Program:
        // 0x00: EBREAK (0x00100073)
        // 0x04: NOP    (0x00000013)
        state.Memory.WriteWord(0, 0x00100073); 
        state.Memory.WriteWord(4, 0x00000013);

        // --- CYCLE 1: FETCH ---
        pipeline.Cycle(); 
        Assert.True(pipeline.IfId.IsValid, "Cycle 1: IF_ID Invalid");
        Assert.Equal((uint)0x00100073, pipeline.IfId.Instruction);
        Console.WriteLine($"Cycle 1 (Fetch): OK. Raw: {pipeline.IfId.Instruction:X}");

        // --- CYCLE 2: DECODE ---
        pipeline.Cycle();
        Assert.NotNull(pipeline.IdEx.DecodedInst);
        
        var decodedType = pipeline.IdEx.DecodedInst.GetType().Name;
        Console.WriteLine($"Cycle 2 (Decode): Inst Type is '{decodedType}'");

        Assert.IsType<EbreakInstruction>(pipeline.IdEx.DecodedInst);

        // --- CYCLE 3: EXECUTE ---
        pipeline.Cycle();
        Assert.NotNull(pipeline.ExMem.DecodedInst);
        Console.WriteLine($"Cycle 3 (Execute): Passed type '{pipeline.ExMem.DecodedInst.GetType().Name}'");
        Assert.IsType<EbreakInstruction>(pipeline.ExMem.DecodedInst);

        // --- CYCLE 4: MEMORY ---
        pipeline.Cycle();
        Assert.NotNull(pipeline.MemWb.DecodedInst);
        Console.WriteLine($"Cycle 4 (Memory): Passed type '{pipeline.MemWb.DecodedInst.GetType().Name}'");
        Assert.IsType<EbreakInstruction>(pipeline.MemWb.DecodedInst);
    }
}
