using Xunit;
using AetherRISC.Core.Architecture;
using AetherRISC.Core.Architecture.Memory;
using AetherRISC.Core.Architecture.Pipeline;
using AetherRISC.Core.Hardware.ISA.Base;
using AetherRISC.Core.Hardware.ISA.Encoding;
using AetherRISC.Core.Helpers;
using System.Collections.Generic;
using System;

namespace AetherRISC.Core.Tests;

public class MemoryIsolationTest
{
    [Fact]
    public void Store_Load_Sequence_Must_Persist_Data()
    {
        // 1. Setup
        var config = SystemConfig.Rv64();
        var state = new MachineState(config);
        state.Memory = new SystemBus(4096);
        var pipeline = new PipelineController(state);
        var asm = new TestAssembler();

        const int sp = 2;
        const int t1 = 6;
        const int val = 10; // Value to store

        // 0x00: Init SP = 100
        asm.Add(pc => Inst.Addi(sp, 0, 100));
        
        // 0x04: Init t1 = 123 (Value to store)
        asm.Add(pc => Inst.Addi(t1, 0, 123));

        // 0x08: SD t1, 0(sp) -> Store 123 to Mem[100]
        asm.Add(pc => Inst.Sd(sp, t1, 0));

        // 0x0C: Clear t1 = 0 (To prove LD works)
        asm.Add(pc => Inst.Addi(t1, 0, 0));

        // 0x10: NOP (Delay to ensure Store completes)
        asm.Add(pc => Inst.Nop());
        asm.Add(pc => Inst.Nop());

        // 0x18: LD t1, 0(sp) -> Load 123 back
        asm.Add(pc => Inst.Ld(t1, sp, 0));

        // 0x1C: NOP (Delay to ensure Load commits)
        asm.Add(pc => Inst.Nop());
        asm.Add(pc => Inst.Nop());
        asm.Add(pc => Inst.Nop());

        // Assemble & Run
        var insts = asm.Assemble();
        for(int i=0; i < insts.Count; i++) 
            state.Memory.WriteWord((uint)(i*4), InstructionEncoder.Encode(insts[i]));

        for(int i=0; i<15; i++) pipeline.Cycle();

        // DIAGNOSTICS
        ulong memVal = state.Memory.ReadDoubleWord(100);
        ulong regVal = state.Registers.Read(t1);

        // Assert 1: Did SD write to memory?
        Assert.Equal((ulong)123, memVal);

        // Assert 2: Did LD write to register?
        Assert.Equal((ulong)123, regVal);
    }
}
