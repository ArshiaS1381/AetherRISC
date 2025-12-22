using Xunit;
using AetherRISC.Core.Architecture;
using AetherRISC.Core.Architecture.Memory;
using AetherRISC.Core.Architecture.Pipeline;
using AetherRISC.Core.Hardware.ISA.Base;
using AetherRISC.Core.Hardware.ISA.Encoding;
using AetherRISC.Core.Helpers;
using System.Collections.Generic;

namespace AetherRISC.Core.Tests;

public class MultiCycleInspectionTest
{
    [Fact]
    public void Trace_ADDI_Through_Full_Pipeline()
    {
        var config = SystemConfig.Rv64();
        var state = new MachineState(config);
        state.Memory = new SystemBus(1024);
        var pipeline = new PipelineController(state);

        // Sequence:
        // 1. ADDI x10, x0, 123
        // 2. ADDI x17, x0, 1
        var encoded1 = InstructionEncoder.Encode(Inst.Addi(10, 0, 123));
        var encoded2 = InstructionEncoder.Encode(Inst.Addi(17, 0, 1));
        
        state.Memory.WriteWord(0, encoded1);
        state.Memory.WriteWord(4, encoded2);

        // --- CYCLE 1: Fetch 1 (ADDI 10) ---
        pipeline.Cycle();
        Assert.Equal(encoded1, pipeline.IfId.Instruction); 

        // --- CYCLE 2: Decode 1, Fetch 2 (ADDI 17) ---
        pipeline.Cycle();
        // Check Decode Latch for Inst 1
        Assert.NotNull(pipeline.IdEx.DecodedInst);
        Assert.Equal(10, pipeline.IdEx.Rd); 
        Assert.True(pipeline.IdEx.RegWrite);
        // Check Fetch Latch for Inst 2
        Assert.Equal(encoded2, pipeline.IfId.Instruction);

        // --- CYCLE 3: Execute 1, Decode 2 ---
        pipeline.Cycle();
        // Check ExMem Latch for Inst 1 (Result 123)
        Assert.Equal((ulong)123, pipeline.ExMem.AluResult);
        Assert.Equal(10, pipeline.ExMem.Rd);
        Assert.True(pipeline.ExMem.RegWrite);
        
        // Check IdEx Latch for Inst 2 (Reg 17)
        Assert.Equal(17, pipeline.IdEx.Rd);

        // --- CYCLE 4: Memory 1, Execute 2 ---
        pipeline.Cycle();
        // Check MemWb Latch for Inst 1
        Assert.Equal((ulong)123, pipeline.MemWb.FinalResult);
        Assert.Equal(10, pipeline.MemWb.Rd);
        Assert.True(pipeline.MemWb.RegWrite);

        // --- CYCLE 5: Writeback 1, Memory 2 ---
        pipeline.Cycle();
        // Inst 1 should now be in Register File
        ulong reg10 = state.Registers.Read(10);
        Assert.Equal((ulong)123, reg10); 
    }
}
