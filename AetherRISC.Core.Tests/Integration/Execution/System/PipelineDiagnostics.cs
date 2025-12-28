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

namespace AetherRISC.Core.Tests.System;

public class PipelineDiagnostics
{
    [Fact]
    public void Diagnostic_Check_IsLoad_Flags()
    {
        // 1. Verify LW is recognized as a Load
        // If this fails, LW instructions are not flagging IsLoad=true
        var lw = new LwInstruction(1, 2, 0); 
        Assert.True(lw.IsLoad, "CRITICAL: LwInstruction.IsLoad is False. Pipeline thinks it is generic ALU op.");
        
        var sw = new SwInstruction(1, 2, 0);
        Assert.True(sw.IsStore, "CRITICAL: SwInstruction.IsStore is False.");
    }

    [Fact]
    public void Diagnostic_Check_Branch_Flushing()
    {
        // 2. Verify Pipeline flushes instructions after a taken branch
        var _ = new InstructionDecoder(); 
        var state = new MachineState(SystemConfig.Rv64());
        state.Memory = new SystemBus(1024);
        var pipeline = new PipelineController(state); state.ProgramCounter = 0;

        // Program:
        // 0x00: BEQ x0, x0, 8  (Jump to 0x08) -> TAKEN
        // 0x04: ADDI x1, x0, 5 (Should be FLUSHED/SKIPPED)
        // 0x08: ADDI x2, x0, 9 (Target)
        
        state.Memory.WriteWord(0, InstructionEncoder.Encode(new BeqInstruction(0, 0, 8))); 
        state.Memory.WriteWord(4, InstructionEncoder.Encode(new AddiInstruction(1, 0, 5)));
        state.Memory.WriteWord(8, InstructionEncoder.Encode(new AddiInstruction(2, 0, 9)));

        // Cycle 1: Fetch BEQ
        pipeline.Cycle();
        
        // Cycle 2: Decode BEQ, Fetch ADDI(x1)
        pipeline.Cycle();
        
        // Cycle 3: Execute BEQ (Taken!), Decode ADDI(x1)
        pipeline.Cycle();
        
        // --- MOMENT OF TRUTH ---
        // At the end of Cycle 3, BEQ executed and set PC=0x08.
        // The ADDI(x1) currently in Decode must be turned into a NOP (Bubble).
        // The Fetch stage should now be looking at 0x08.

        // Check 1: Did PC update?
        Assert.Equal((ulong)0x08, state.ProgramCounter);

        // Check 2: Was the Decode latch flushed?
        // If this is ADDI, flushing failed. It should be NOP/Null.
        bool isAddi = pipeline.IdEx.DecodedInst is AddiInstruction;
        Assert.False(isAddi, "CRITICAL: Branch was taken, but the next instruction (ADDI) is still in the pipeline! Logic missing.");

        // Cycle 4: Execute the Flushed Bubble
        pipeline.Cycle();
        Assert.Equal((ulong)0, state.Registers.Read(1)); // x1 should NOT be 5
    }
}




