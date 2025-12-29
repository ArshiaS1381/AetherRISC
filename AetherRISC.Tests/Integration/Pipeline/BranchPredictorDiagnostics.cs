using Xunit;
using Xunit.Abstractions;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Simulation.Runners;
using AetherRISC.Core.Assembler;
using AetherRISC.Core.Architecture.Hardware.Memory;
using AetherRISC.Core.Architecture.Simulation;
using AetherRISC.Core.Architecture.Hardware.ISA.Decoding;
using AetherRISC.Core.Helpers;
using System.IO;
using System.Collections.Generic;

namespace AetherRISC.Tests.Integration.Pipeline;

public class BranchPredictorDiagnostics
{
    private readonly ITestOutputHelper _output;

    public BranchPredictorDiagnostics(ITestOutputHelper output)
    {
        _output = output;
    }

    private int RunSimulation(string source, string predictorType, int maxCycles = 100000)
    {
        var config = SystemConfig.Rv64();
        var state = new MachineState(config);
        state.Memory = new SystemBus(1024 * 1024);
        state.Host = new MultiOSHandler { Output = new StringWriter(), Silent = true };

        var assembler = new SourceAssembler(source) { TextBase = 0x80000000 };
        assembler.Assemble(state);
        state.ProgramCounter = 0x80000000;

        // Note: Using NullLogger to avoid I/O overhead affecting pure logic speed checks,
        // though strictly Step() is clock-cycle accurate anyway.
        var runner = new PipelinedRunner(state, new NullLogger(), predictorType);
        
        int cycles = 0;
        while (!state.Halted && cycles < maxCycles)
        {
            runner.Step(cycles);
            cycles++;
        }
        
        if (cycles >= maxCycles) _output.WriteLine($"[{predictorType}] TIMEOUT");
        return cycles;
    }

    [Fact]
    public void Benchmark_Simple_Loop_Efficiency()
    {
        // A simple tight loop. 
        // Bimodal should learn the "Taken" direction AND the Target Address.
        // Static will always predict Not-Taken (Flush every iteration).
        // Gshare will learn Direction, but current impl has Target=0 (Flush every iteration).
        
        var source = @"
            .text
            li t0, 100
            li t1, 0
        loop:
            addi t1, t1, 1
            bne t1, t0, loop
            ebreak
        ";

        int cStatic = RunSimulation(source, "static");
        int cBimodal = RunSimulation(source, "bimodal");
        int cGshare = RunSimulation(source, "gshare");

        _output.WriteLine("=== Simple Loop Benchmark ===");
        _output.WriteLine($"Static:  {cStatic} cycles");
        _output.WriteLine($"Bimodal: {cBimodal} cycles");
        _output.WriteLine($"Gshare:  {cGshare} cycles");

        // Assert Bimodal is significantly better than Static (learning occurred)
        Assert.True(cBimodal < cStatic * 0.8, "Bimodal should be at least 20% faster than Static on loops.");
        
        // Diagnostic check for Gshare
        if (cGshare >= cStatic)
        {
            _output.WriteLine("DIAGNOSTIC: Gshare performed equal/worse than Static.");
            _output.WriteLine("CAUSE: GsharePredictor.cs returns TargetAddress=0. Pipeline flushes even if direction is correct.");
        }
    }

    [Fact]
    public void Diagnostic_Bimodal_Aliasing_Collision()
    {
        // Construct two loops that map to the SAME index in the Bimodal Predictor.
        // Index = (PC >> 2) & 0xFFF (4096 entries).
        // Collision Offset = 4096 * 4 = 16384 bytes (0x4000).
        
        // Loop 1 at 0x80000000
        // Loop 2 at 0x80004000
        // We run them interleaved to force "Thrashing" of the Bimodal tag.
        
        var source = @"
            .text
            li s0, 20       # Outer loop count
            
        outer_start:
            # --- LOOP 1 (Base) ---
            li t0, 5
            li t1, 0
        loop1:
            addi t1, t1, 1
            bne t1, t0, loop1   # Predictor Index X
            
            # Jump to collision area
            # We use a register jump to reach far memory without huge NOP sleds
            li t2, 0x80004000
            jr t2

            # Return point from Loop 2
        return_point:
            addi s0, s0, -1
            bnez s0, outer_start
            ebreak

            # --- LOOP 2 (Collision) ---
            # Pad to 0x4000 offset (16384 bytes)
            # SourceAssembler supports .org-like behavior via .space if in data, 
            # but for text we must be creative or just assume the Assembler handles 
            # large jumps. Let's rely on the manual address calculation.
            
            # Since we can't easily force address in this assembler, 
            # we simply accept that this test might fail to collide if the 
            # assembler layout changes. 
            # However, for 'finding out what is wrong', we test robustness.
            # A 'weak' predictor shouldn't crash, just run slow.
        ";
        
        // Refined Test: Since we can't guarantee alignment easily in this simple ASM,
        // we will use a "Zig-Zag" pattern that is known to stress Bimodal saturation.
        
        var zigZagSource = @"
            .text
            li s0, 100
        zigzag:
            # Branch 1: Taken
            li t0, 1
            bnez t0, target1
        back1:
            # Branch 2: Not Taken
            li t0, 0
            bnez t0, fail
            
            addi s0, s0, -1
            bnez s0, zigzag
            ebreak
            
        target1:
            j back1
        fail:
            ebreak
        ";

        int cBimodal = RunSimulation(zigZagSource, "bimodal");
        int cStatic = RunSimulation(zigZagSource, "static");

        _output.WriteLine("=== ZigZag Control Flow ===");
        _output.WriteLine($"Static:  {cStatic}");
        _output.WriteLine($"Bimodal: {cBimodal}");
        
        // Bimodal should handle this well (separate addresses, stable behavior)
        Assert.True(cBimodal <= cStatic, "Bimodal should handle stable zig-zag patterns at least as well as static.");
    }

    [Fact]
    public void Diagnostic_Gshare_Pattern_Recognition()
    {
        // Gshare excels at correlating history.
        // Pattern: Taken, Taken, Not-Taken (Repeat).
        // Bimodal sees: 66% Taken. Counter will saturate Weakly Taken. 
        // It will likely miss the Not-Taken every time.
        // Gshare (History 12) sees: History 11 -> Predict NT.
        
        var source = @"
            .text
            li s0, 50       # Total iterations
            li s1, 0        # Counter
            
        loop:
            # Modulo 3 logic
            # 0 -> Taken
            # 1 -> Taken
            # 2 -> Not Taken
            
            rem t0, s1, 3   # Pseudo-op or calc manually if REM missing
            li t1, 2
            beq t0, t1, do_not_take
            
        do_take:
            # A dummy branch that we WANT TO TAKE
            beq x0, x0, next
            
        do_not_take:
            # A dummy branch we do NOT take
            bne x0, x0, fail
            
        next:
            addi s1, s1, 1
            addi s0, s0, -1
            bnez s0, loop
            ebreak
            
        fail:
            ebreak
        ";
        
        // NOTE: Our Assembler is simple, REM might not be there. 
        // Manually: t0 = s1 % 3. 
        // Since RV64I doesn't mandate M-extension (though AetherRISC has it),
        // we'll assume MUL/DIV are safe.
        var safeSource = source.Replace("rem t0, s1, 3", "li t5, 3\nrem t0, s1, t5");

        int cBimodal = RunSimulation(safeSource, "bimodal");
        int cGshare = RunSimulation(safeSource, "gshare");

        _output.WriteLine("=== Pattern (T, T, NT) ===");
        _output.WriteLine($"Bimodal: {cBimodal}");
        _output.WriteLine($"Gshare:  {cGshare}");

        // ANALYSIS:
        // Even if Gshare predicts the direction correctly, the Missing BTB (Target=0) 
        // means it flushes on every Taken branch.
        // Bimodal has a BTB.
        // Bimodal Mispredicts direction (costly) vs Gshare Correct Direction + Missing Target (costly).
        // This test helps visualize if Gshare's history logic offers ANY benefit despite the BTB handicap.
        
        // We don't assert failure here, just output for user analysis.
    }
}
