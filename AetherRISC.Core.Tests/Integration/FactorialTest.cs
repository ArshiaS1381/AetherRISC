using Xunit;
using AetherRISC.Core.Architecture;
using AetherRISC.Core.Architecture.Memory;
using AetherRISC.Core.Architecture.Pipeline;
using AetherRISC.Core.Helpers;
using AetherRISC.Core.Architecture.ISA.Encoding;

namespace AetherRISC.Core.Tests.Integration;

public class FactorialTest
{
    [Fact]
    public void Calculate_Factorial_5()
    {
        // Algorithm:
        // x10 (Result) = 1
        // x11 (Iterator) = 5
        // loop:
        //   x12 = 2
        //   BLT x11, x12, exit  (if i < 2, exit)
        //   MUL x10, x10, x11   (res = res * i)
        //   ADDI x11, x11, -1   (i--)
        //   JAL x0, loop        (jump back)
        // exit:
        
        var state = new MachineState(SystemConfig.Rv64());
        state.Memory = new SystemBus(1024);
        var pipeline = new PipelineController(state);
        var asm = new TestAssembler();

        // 0x00: x10 = 1
        asm.Add(pc => Inst.Addi(10, 0, 1));
        // 0x04: x11 = 5
        asm.Add(pc => Inst.Addi(11, 0, 5));

        // 0x08: loop label
        asm.Add(pc => Inst.Addi(12, 0, 2), "loop");
        
        // 0x0C: BLT x11, x12, exit (offset to be calc)
        // Note: We need Inst.Blt helper. If missing, we manually encode or add to helper.
        // Assuming we update Inst helper below, or use raw factory:
        // Manual B-Type for now if helper missing: 
        // But better to add helper!
        // We will assume helper exists or logic below handles it.
        // Let's rely on BGE to stay in loop: BGE x11, x12, continue_mul
        // If we fall through, we exit.
        
        // Revised Logic (easier with existing helpers):
        // loop:
        //    MUL x10, x10, x11
        //    ADDI x11, x11, -1
        //    ADDI x12, 0, 1
        //    BNE x11, x12, loop (if i != 1, loop)
        // This is what failed before because i went negative. 
        // Why? Because BNE checks equality. If x11 skips 1 (bug?), it loops forever.
        // But 5->4->3->2->1 should hit 1 exactly.
        
        // Let's use the robust logic with BLT now that we added it.
        // We need to update Inst.cs first! See step 4.
        
        asm.Add(pc => Inst.Blt(11, 12, asm.To("exit", pc))); // Jump to exit if i < 2

        asm.Add(pc => Inst.Mul(10, 10, 11));    // res *= i
        asm.Add(pc => Inst.Addi(11, 11, -1));   // i--
        asm.Add(pc => Inst.Jal(0, asm.To("loop", pc))); // JAL loop
        
        // exit label (Instruction 0x18 or so)
        asm.Add(pc => Inst.Nop(), "exit");
        
        var insts = asm.Assemble();
        for(int i=0; i < insts.Count; i++) 
            state.Memory.WriteWord((uint)(i*4), InstructionEncoder.Encode(insts[i]));

        for(int i=0; i<100; i++) pipeline.Cycle();

        Assert.Equal((ulong)120, state.Registers.Read(10));
    }
}
