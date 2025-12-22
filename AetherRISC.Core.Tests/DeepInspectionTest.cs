using Xunit;
using AetherRISC.Core.Architecture;
using AetherRISC.Core.Architecture.Memory;
using AetherRISC.Core.Architecture.Pipeline;
using AetherRISC.Core.Hardware.ISA.Base;
using AetherRISC.Core.Hardware.ISA.Encoding;
using AetherRISC.Core.Helpers;
using System.Collections.Generic;

namespace AetherRISC.Core.Tests;

public class DeepInspectionTest
{
    [Fact]
    public void Inspect_Encoder_And_Pipeline_Propagation()
    {
        var config = SystemConfig.Rv64();
        var state = new MachineState(config);
        state.Memory = new SystemBus(1024);
        var pipeline = new PipelineController(state);

        // 1. ENCODER CHECK
        // Target: ADDI x10, x0, 123
        // Bin: 000001111011 00000 000 01010 0010011
        // Hex: 0x07B00513
        var instruction = Inst.Addi(10, 0, 123);
        uint encoded = InstructionEncoder.Encode(instruction);
        
        // Assert 1: Is the encoder outputting garbage?
        Assert.True(encoded != 0, "Encoder returned 0 (NOP) for ADDI!");
        Assert.Equal((uint)0x07B00513, encoded);

        // 2. MEMORY CHECK
        state.Memory.WriteWord(0, encoded);
        uint fetched = state.Memory.ReadWord(0);
        Assert.Equal(encoded, fetched);

        // 3. PIPELINE STEP-BY-STEP CHECK

        // CYCLE 1: Fetch
        pipeline.Cycle(); 
        // Check IF_ID Latch
        Assert.True(pipeline.IfId.IsValid, "Fetch Stage failed to validate IF_ID");
        Assert.Equal(encoded, pipeline.IfId.Instruction);

        // CYCLE 2: Decode
        pipeline.Cycle();
        // Check ID_EX Latch
        var decoded = pipeline.IdEx.DecodedInst;
        Assert.NotNull(decoded);
        Assert.IsType<AddiInstruction>(decoded);
        Assert.Equal(10, pipeline.IdEx.Rd);
        Assert.True(pipeline.IdEx.RegWrite, "Decode Stage failed to set RegWrite=true");

        // CYCLE 3: Execute
        pipeline.Cycle();
        // Check EX_MEM Latch
        Assert.Equal((ulong)123, pipeline.ExMem.AluResult);
        Assert.Equal(10, pipeline.ExMem.Rd);
        Assert.True(pipeline.ExMem.RegWrite, "Execute Stage dropped RegWrite signal");

        // CYCLE 4: Memory
        pipeline.Cycle();
        // Check MEM_WB Latch
        Assert.Equal((ulong)123, pipeline.MemWb.FinalResult);
        Assert.Equal(10, pipeline.MemWb.Rd);
        Assert.True(pipeline.MemWb.RegWrite, "Memory Stage dropped RegWrite signal");

        // CYCLE 5: Writeback
        pipeline.Cycle();
        // Check Register File
        ulong regVal = state.Registers.Read(10);
        Assert.Equal((ulong)123, regVal);
    }
}
