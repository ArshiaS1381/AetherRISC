using Xunit;
using AetherRISC.Core.Architecture;
using AetherRISC.Core.Architecture.Memory;
using AetherRISC.Core.Architecture.Pipeline;
using AetherRISC.Core.Architecture.ISA.Encoding;
using AetherRISC.Core.Helpers;
using AetherRISC.Core.Tests.Integration;

namespace AetherRISC.Core.Tests.Architecture.System;

public class DiagnosticTests
{
    private MachineState _state;
    private PipelineController _pipeline;
    private TestAssembler _asm;

    public DiagnosticTests() {
        _state = new MachineState(SystemConfig.Rv64());
        _state.Memory = new SystemBus(1024);
        _pipeline = new PipelineController(_state);
        _asm = new TestAssembler();
    }

    [Fact]
    public void RegisterFile_SanityCheck()
    {
        _state.Registers.Write(1, 0xDEADBEEF);
        Assert.Equal(0xDEADBEEFu, _state.Registers.Read(1));

        _state.Registers.Write(0, 0xDEADBEEF);
        Assert.Equal(0u, _state.Registers.Read(0));
    }

    [Fact]
    public void Pipeline_Timing_Probe_ALU_to_ALU()
    {
        // 1. Initialize x1
        _state.Registers.Write(1, 10);

        // 2. ADDI x1, x0, 20
        _asm.Add(pc => Inst.Addi(1, 0, 20));

        // 3. ADD x2, x1, x0
        _asm.Add(pc => Inst.Add(2, 1, 0));

        LoadAndRun(cycles: 6); 

        Assert.Equal(20u, _state.Registers.Read(1));
        Assert.Equal(20u, _state.Registers.Read(2));
    }

    [Fact]
    public void Pipeline_Timing_Probe_Load_to_ALU()
    {
        // 1. Setup Memory
        _state.Memory!.WriteWord(100, 42);

        // 2. LW x1, 100(x0)
        _asm.Add(pc => Inst.Lw(1, 0, 100));

        // 3. ADDI x2, x1, 5
        _asm.Add(pc => Inst.Addi(2, 1, 5));

        LoadInstructions();

        // Cycle 1: Fetch LW
        _pipeline.Cycle(); 
        
        // Cycle 2: Decode LW, Fetch ADDI
        _pipeline.Cycle();

        // Cycle 3: Execute LW, Decode ADDI
        // LW executes, writes 42 to x1 immediately.
        _pipeline.Cycle();
        
        // CHECKPOINT 1: Register should be updated immediately after Execute
        Assert.Equal(42u, _state.Registers.Read(1));

        // Cycle 4: Memory LW, Execute ADDI
        // ADDI executes. It reads x1. If x1 is 42, result is 47.
        _pipeline.Cycle();
        
        // Cycle 5: Writeback LW, Memory ADDI
        _pipeline.Cycle();

        // Cycle 6: Writeback ADDI
        _pipeline.Cycle();

        Assert.Equal(42u, _state.Registers.Read(1));
        Assert.Equal(47u, _state.Registers.Read(2));
    }

    private void LoadInstructions() {
        var insts = _asm.Assemble();
        for(int i=0; i < insts.Count; i++) 
            _state.Memory!.WriteWord((uint)(i*4), InstructionEncoder.Encode(insts[i]));
    }

    private void LoadAndRun(int cycles) {
        LoadInstructions();
        for(int i=0; i<cycles; i++) _pipeline.Cycle();
    }
}
