using AetherRISC.Core.Architecture.Simulation.Runners;
using Xunit;
using System.Collections.Generic;
using AetherRISC.Core.Architecture;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.Memory;
using AetherRISC.Core.Abstractions.Interfaces;
using AetherRISC.Core.Architecture.Hardware.ISA.Instructions;
using AetherRISC.Core.Architecture.Hardware.ISA.Encoding;
using AetherRISC.Core.Architecture.Hardware.ISA.Decoding;

namespace AetherRISC.Core.Tests.System;

public class PipelinedRunnerTests
{
    // --- Mock Logger for Verification ---
    private class MockLogger : ISimulationLogger
    {
        public int CyclesCounted { get; private set; } = 0;
        public List<string> Events { get; } = new();

        public void Initialize(string programName) { }
        public void FinalizeSession() { }
        public void BeginCycle(int cycle) { CyclesCounted++; }
        public void CompleteCycle() { }
        public void Log(string tag, string message) { }

        public void LogStageFetch(ulong pc, uint raw) => Events.Add($"IF:{pc:X}");
        public void LogStageDecode(ulong pc, uint raw, IInstruction inst) => Events.Add($"ID:{inst.Mnemonic}");
        public void LogStageExecute(ulong pc, uint raw, string info) => Events.Add($"EX:{info}");
        public void LogStageMemory(ulong pc, uint raw, string info) => Events.Add($"MEM:{info}");
        public void LogStageWriteback(ulong pc, uint raw, int rd, ulong val) => Events.Add($"WB:x{rd}={val}");
        public void LogRegistersState(ulong[] registers) { }
    }

    [Fact]
    public void Run_Should_Stop_After_MaxCycles()
    {
        // Force Encoders to load
        var _ = new InstructionDecoder(); 

        var state = new MachineState(SystemConfig.Rv64());
        state.Memory = new SystemBus(1024);
        var logger = new MockLogger();
        state.ProgramCounter = 0; var runner = new PipelinedRunner(state, logger);

        runner.Run(5);

        Assert.Equal(5, logger.CyclesCounted);
    }

    [Fact]
    public void Run_Should_Execute_Instructions_Correctly()
    {
        var _ = new InstructionDecoder(); 

        var state = new MachineState(SystemConfig.Rv64());
        state.Memory = new SystemBus(1024);
        var logger = new MockLogger();
        
        // Program: ADDI x1, x0, 10 (0x00A00093)
        uint inst = InstructionEncoder.Encode(new AddiInstruction(1, 0, 10));
        state.Memory.WriteWord(0, inst);

        state.ProgramCounter = 0; var runner = new PipelinedRunner(state, logger);
        
        runner.Run(5);

        Assert.Equal((ulong)10, state.Registers.Read(1));
    }

    [Fact]
    public void Run_Should_Log_Pipeline_Stages()
    {
        var _ = new InstructionDecoder();

        var state = new MachineState(SystemConfig.Rv64());
        state.Memory = new SystemBus(1024);
        var logger = new MockLogger();
        
        // 0x00: NOP
        state.Memory.WriteWord(0, 0x00000013);

        state.ProgramCounter = 0; var runner = new PipelinedRunner(state, logger);
        runner.Run(1);

        Assert.Contains("IF:0", logger.Events);
        Assert.Contains("ID:NOP", logger.Events); 
    }

    [Fact]
    public void Run_Should_Halt_On_EBREAK()
    {
        // FIX: Must instantiate Decoder to register EBREAK encoder logic
        var _ = new InstructionDecoder();

        var state = new MachineState(SystemConfig.Rv64());
        state.Memory = new SystemBus(1024);
        var logger = new MockLogger();
        
        // 0: ADDI x1, x0, 1
        // 4: EBREAK
        // 8: ADDI x1, x0, 5 (Should not be retired/committed)
        
        state.Memory.WriteWord(0, InstructionEncoder.Encode(new AddiInstruction(1, 0, 1)));
        state.Memory.WriteWord(4, InstructionEncoder.Encode(new EbreakInstruction()));
        state.Memory.WriteWord(8, InstructionEncoder.Encode(new AddiInstruction(1, 0, 5)));

        state.ProgramCounter = 0; var runner = new PipelinedRunner(state, logger);
        
        // Run with ample cycles (20), but it should stop early (~8 cycles)
        runner.Run(20);

        Assert.True(logger.CyclesCounted < 20, $"Simulator did not halt. Cycles: {logger.CyclesCounted}");
        Assert.Equal((ulong)1, state.Registers.Read(1));
    }
}
