using Xunit;
using AetherRISC.Core.Architecture;
using AetherRISC.Core.Architecture.Memory;
using AetherRISC.Core.Architecture.Pipeline;
using AetherRISC.Core.Architecture.ISA.Base;
using AetherRISC.Core.Architecture.ISA.Encoding;
using AetherRISC.Core.Helpers;
using System.Collections.Generic;

namespace AetherRISC.Core.Tests;

public class DiagnosticTests
{
    [Fact]
    public void JAL_Should_Create_Stack_Frame_And_Return()
    {
        var config = SystemConfig.Rv64();
        var state = new MachineState(config);
        state.Memory = new SystemBus(4096);
        var pipeline = new PipelineController(state);
        var asm = new TestAssembler();
        
        // 0x00: ADDI sp, x0, 100
        asm.Add(pc => Inst.Addi(2, 0, 100));            
        // 0x04: JAL ra, +8 (Call Func)
        asm.Add(pc => Inst.Jal(1, 8));                  
        // 0x08: JAL x0, +100 (Halt loop)
        asm.Add(pc => Inst.Jal(0, 0x100));              
        
        // Func (0x0C): ADDI sp, sp, -16
        asm.Add(pc => Inst.Addi(2, 2, -16));            
        // 0x10: SD ra, 8(sp) -> CORRECTED: Rs1=2(SP), Rs2=1(RA)
        asm.Add(pc => Inst.Sd(2, 1, 8));                
        // 0x14: Destroy RA
        asm.Add(pc => Inst.Addi(1, 0, 0));              
        // 0x18: LD ra, 8(sp) -> CORRECTED: Rd=1(RA), Rs1=2(SP)
        asm.Add(pc => Inst.Ld(1, 2, 8));                
        // 0x1C: Restore SP
        asm.Add(pc => Inst.Addi(2, 2, 16));             
        // 0x20: RET
        asm.Add(pc => Inst.Jalr(0, 1, 0));              

        var insts = asm.Assemble();
        for(int i=0; i < insts.Count; i++) 
            state.Memory.WriteWord((uint)(i*4), InstructionEncoder.Encode(insts[i]));

        for(int i=0; i<20; i++) pipeline.Cycle();

        Assert.Equal((ulong)0x08, state.Registers.Read(1));
    }
}
