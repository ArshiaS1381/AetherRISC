using AetherRISC.Core.Architecture.Hardware.Registers;
using Xunit;
// NEW IMPORTS
using AetherRISC.Core.Architecture.Hardware.Memory;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.Pipeline.Controller;
using AetherRISC.Core.Architecture.Hardware.ISA.Encoding;
using AetherRISC.Core.Architecture.Hardware.ISA.Instructions;
using AetherRISC.Core.Architecture.Hardware.ISA.Decoding;

namespace AetherRISC.Core.Tests.Architecture.System;

public class DiagnosticTests
{
    private MachineState _state;
    private PipelineController _pipeline;

    public DiagnosticTests() {
        var _ = new InstructionDecoder(); 
        _state = new MachineState(SystemConfig.Rv64());
        _state.Memory = new SystemBus(1024);
        _pipeline = new PipelineController(_state);
    }

    [Fact]
    public void RegisterFile_SanityCheck()
    {
        _state.Registers.Write(1, 0xDEADBEEF);
        Assert.Equal(0xDEADBEEFu, _state.Registers.Read(1));
    }

    [Fact]
    public void Pipeline_Timing_Probe_Load_to_ALU()
    {
        // 1. Setup Memory Data
        _state.Memory!.WriteWord(100, 42);

        // 2. Program Setup
        uint i_lw = InstructionEncoder.Encode(new LwInstruction(1, 0, 100));
        uint i_addi = InstructionEncoder.Encode(new AddiInstruction(2, 1, 5));

        _state.Memory!.WriteWord(0x80000000, i_lw);
        _state.Memory!.WriteWord(0x80000004, i_addi);
        _state.ProgramCounter = 0x80000000;

        // Run 5 cycles (Fetch -> Decode -> Exec(Stall) -> Exec -> Mem -> WB)
        for(int i=0; i<7; i++) _pipeline.Cycle();

        // VERIFY: ADDI finished (42 + 5 = 47)
        Assert.Equal(47u, _state.Registers.Read(2)); 
    }
}



