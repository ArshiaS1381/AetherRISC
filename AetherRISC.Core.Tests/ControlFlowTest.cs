using Xunit;
using AetherRISC.Core.Architecture;
using AetherRISC.Core.Architecture.Memory;
using AetherRISC.Core.Architecture.Pipeline;
using AetherRISC.Core.Architecture.ISA.Encoding;
using AetherRISC.Core.Helpers;
using System.Collections.Generic;
using System.Text;

namespace AetherRISC.Core.Tests;

public class ControlFlowTest
{
    [Fact]
    public void Debug_Control_Flow()
    {
        var config = SystemConfig.Rv64();
        var state = new MachineState(config);
        state.Memory = new SystemBus(1024);
        var pipeline = new PipelineController(state);

        var insts = new List<uint>();
        
        // 0x00: JAL x0, 8 (Jump over trap)
        insts.Add(0x0080006F); 
        // 0x04: Trap 1
        insts.Add(InstructionEncoder.Encode(Inst.Addi(10, 0, 1)));
        // 0x08: x11 = 10
        insts.Add(InstructionEncoder.Encode(Inst.Addi(11, 0, 10)));
        // 0x0C: x12 = 10
        insts.Add(InstructionEncoder.Encode(Inst.Addi(12, 0, 10)));
        // 0x10: BNE x11, x12, 8 (Should NOT branch)
        insts.Add(InstructionEncoder.Encode(Inst.Bne(11, 12, 8))); 
        // 0x14: Success Marker 1 (x10 = 2)
        insts.Add(InstructionEncoder.Encode(Inst.Addi(10, 0, 2)));
        // 0x18: x12 = 20
        insts.Add(InstructionEncoder.Encode(Inst.Addi(12, 0, 20)));
        // 0x1C: BNE x11, x12, 8 (Should branch to 0x24)
        insts.Add(InstructionEncoder.Encode(Inst.Bne(11, 12, 8)));
        // 0x20: Trap 2
        insts.Add(InstructionEncoder.Encode(Inst.Addi(10, 0, 3)));
        // 0x24: Final Success (x10 = 42)
        insts.Add(InstructionEncoder.Encode(Inst.Addi(10, 0, 42)));

        for(int i=0; i < insts.Count; i++) 
            state.Memory.WriteWord((uint)(i*4), insts[i]);

        for(int i=1; i<=30; i++) pipeline.Cycle();

        Assert.Equal((ulong)42, state.Registers.Read(10));
    }
}
