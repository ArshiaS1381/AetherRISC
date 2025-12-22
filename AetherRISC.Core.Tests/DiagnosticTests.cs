using Xunit;
using AetherRISC.Core.Architecture;
using AetherRISC.Core.Architecture.Memory;
using AetherRISC.Core.Architecture.Pipeline;
using AetherRISC.Core.Hardware.ISA.Base;
using AetherRISC.Core.Hardware.ISA.Decoding;
using AetherRISC.Core.Hardware.ISA.Encoding;
using AetherRISC.Core.Helpers;
using System.Collections.Generic;
using System;
using AetherRISC.Core.Abstractions.Interfaces;

namespace AetherRISC.Core.Tests;

// Internal helper to replace AetherRISC.CLI.LabelAssembler
public class TestAssembler
{
    private readonly List<(string label, Func<int, IInstruction> factory)> _lines = new();
    private readonly Dictionary<string, int> _labels = new();

    public void Add(Func<int, IInstruction> factory, string label = null)
    {
        if (!string.IsNullOrEmpty(label)) _labels[label] = _lines.Count * 4;
        _lines.Add((label, factory));
    }

    public List<IInstruction> Assemble()
    {
        var result = new List<IInstruction>();
        for (int i = 0; i < _lines.Count; i++) result.Add(_lines[i].factory(i * 4));
        return result;
    }
    
    public int To(string label, int currentPc) => _labels.ContainsKey(label) ? _labels[label] - currentPc : 0;
}

public class DiagnosticTests
{
    [Fact]
    public void JAL_Should_Sign_Extend_Negative_Immediates()
    {
        // TARGET: Fixes the 'Massive x1 Value' bug
        var decoder = new InstructionDecoder();
        // JAL x0, -4 -> Hex: 0xFFDFF06F
        var inst = (JalInstruction)decoder.Decode(0xFFDFF06F); 
        Assert.Equal(-4, inst.Imm);
    }

    [Fact]
    public void Pipeline_Should_Flush_On_Branch_Taken()
    {
        // TARGET: Fixes the 'Infinite Loop' bug
        var state = new MachineState(SystemConfig.Rv64());
        state.Memory = new SystemBus(4096);
        var pipeline = new PipelineController(state);

        // 0x00: ADDI x1, x0, 1
        // 0x04: BNE  x1, x0, +8  (Taken -> Jump to 0x0C)
        // 0x08: ADDI x2, x0, 99  (GHOST - Should be flushed)
        // 0x0C: ADDI x3, x0, 5   (Target)
        
        state.Memory.WriteWord(0x00, InstructionEncoder.Encode(Inst.Addi(1, 0, 1)));
        state.Memory.WriteWord(0x04, InstructionEncoder.Encode(Inst.Bne(1, 0, 8))); 
        state.Memory.WriteWord(0x08, InstructionEncoder.Encode(Inst.Addi(2, 0, 99)));
        state.Memory.WriteWord(0x0C, InstructionEncoder.Encode(Inst.Addi(3, 0, 5)));

        // Run 20 cycles to ensure the Writeback stage completes
        for(int i=0; i<20; i++) pipeline.Cycle();

        Assert.Equal((ulong)1, state.Registers.Read(1)); // Init ran
        Assert.Equal((ulong)5, state.Registers.Read(3)); // Target reached
        
        // CRITICAL CHECK: x2 must be 0. If it is 99, the flush failed.
        Assert.Equal((ulong)0, state.Registers.Read(2)); 
    }

    [Fact]
    public void Recursive_Function_Should_Preserve_Return_Address()
    {
        // TARGET: Verifies Stack/Ret logic
        var state = new MachineState(SystemConfig.Rv64());
        state.Memory = new SystemBus(4096);
        var pipeline = new PipelineController(state);

        // USE THE LOCAL TEST ASSEMBLER, NOT THE CLI ONE
        var asm = new TestAssembler();
        
        // 0x00: ADDI sp, x0, 100
        asm.Add(pc => Inst.Addi(2, 0, 100));            
        // 0x04: JAL ra, +8 (Call Func)
        asm.Add(pc => Inst.Jal(1, 8));                  
        // 0x08: JAL x0, +100 (Halt loop)
        asm.Add(pc => Inst.Jal(0, 0x100));              
        
        // Func:
        // 0x0C: ADDI sp, sp, -16
        asm.Add(pc => Inst.Addi(2, 2, -16));            
        // 0x10: SD ra, 8(sp)
        asm.Add(pc => Inst.Sd(1, 2, 8));                
        // 0x14: ADDI ra, x0, 0 (Destroy RA)
        asm.Add(pc => Inst.Addi(1, 0, 0));              
        // 0x18: LD ra, 8(sp) (Restore RA)
        asm.Add(pc => Inst.Ld(1, 2, 8));                
        // 0x1C: ADDI sp, sp, 16
        asm.Add(pc => Inst.Addi(2, 2, 16));             
        // 0x20: JALR x0, ra, 0 (Return)
        asm.Add(pc => Inst.Jalr(0, 1, 0));              

        var insts = asm.Assemble();
        for(int i=0; i < insts.Count; i++) 
            state.Memory.WriteWord((uint)(i*4), InstructionEncoder.Encode(insts[i]));

        for(int i=0; i<30; i++) pipeline.Cycle();

        // If we successfully returned, we are trapped in the halt loop at 0x108+
        // If we crashed or didn't jump back, PC would be something else
        Assert.True(state.ProgramCounter > 0x20);
    }
}
