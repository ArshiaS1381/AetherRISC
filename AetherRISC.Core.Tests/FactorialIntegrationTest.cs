using Xunit;
using AetherRISC.Core.Architecture;
using AetherRISC.Core.Architecture.Memory;
using AetherRISC.Core.Architecture.Pipeline;
using AetherRISC.Core.Hardware.ISA.Base;
using AetherRISC.Core.Hardware.ISA.Encoding;
using AetherRISC.Core.Helpers;
using AetherRISC.Core.Abstractions.Interfaces;
using System.Collections.Generic;
using System;

namespace AetherRISC.Core.Tests;

public class DebugHost : ISystemCallHandler
{
    public List<long> IntOutputs { get; } = new();
    public void PrintInt(long value) => IntOutputs.Add(value);
    public void PrintString(string value) { }
    public void Exit(int code) { }
}

public class FactorialIntegrationTest
{
    [Fact]
    public void SanityCheck_Assembler_Forward_Labels()
    {
        var asm = new TestAssembler();
        // 0x00: JAL x0, target
        asm.Add(pc => Inst.Jal(0, asm.To("target", pc)));
        // 0x04: NOP
        asm.Add(pc => Inst.Nop());
        // 0x08: target: ADD x0, x0, x0
        asm.Add(pc => Inst.Add(0, 0, 0), label: "target");

        var insts = asm.Assemble();
        var jal = (JalInstruction)insts[0];

        // Should jump 8 bytes forward (from 0x00 to 0x08)
        Assert.True(jal.Imm == 8, $"Assembler resolved offset as {jal.Imm} instead of 8");
    }

    [Fact]
    public void Run_Full_Factorial_With_Trace()
    {
        var config = SystemConfig.Rv64();
        var state = new MachineState(config);
        state.Memory = new SystemBus(4096);
        var pipeline = new PipelineController(state);
        var host = new DebugHost();
        state.Host = host;

        var asm = new TestAssembler();
        
        // Reg Aliases
        const int x0=0, ra=1, sp=2, a0=10, a7=17, t0=5, t1=6;

        // 0x00: Bootstrap (Jal to main)
        asm.Add(pc => Inst.Jal(x0, asm.To("main", pc))); 

        // 0x04: Halt Loop
        asm.Add(pc => Inst.Jal(x0, 0), label: "halt");

        // 0x08: Main Entry
        asm.Add(pc => Inst.Addi(sp, x0, 2000), label: "main");
        asm.Add(pc => Inst.Addi(a0, x0, 5));          
        asm.Add(pc => Inst.Jal(ra, asm.To("fact", pc))); 
        asm.Add(pc => Inst.Jal(x0, asm.To("halt", pc)));

        // Factorial Function
        asm.Add(pc => Inst.Addi(sp, sp, -32), label: "fact");
        asm.Add(pc => Inst.Sd(sp, ra, 16));
        asm.Add(pc => Inst.Sd(sp, a0, 8)); // Store n

        // DEBUG: Print current 'n' (a0)
        asm.Add(pc => Inst.Addi(a7, x0, 1)); // PrintInt
        asm.Add(pc => Inst.Ecall());

        // Base Case Check
        asm.Add(pc => Inst.Addi(t0, x0, 2));
        asm.Add(pc => Inst.Slt(t0, a0, t0));
        asm.Add(pc => Inst.Bne(t0, x0, asm.To("base_case", pc)));

        // Recursion
        asm.Add(pc => Inst.Addi(a0, a0, -1));
        asm.Add(pc => Inst.Jal(ra, asm.To("fact", pc)));

        // Restore
        asm.Add(pc => Inst.Ld(t1, sp, 8)); // Load n into t1
        
        // --- FIXED DEBUG SEQUENCE ---
        asm.Add(pc => Inst.Addi(30, a0, 0)); // t2(x30) = a0 (save result)
        asm.Add(pc => Inst.Addi(a0, t1, 0)); // Move t1 to a0 to print
        asm.Add(pc => Inst.Addi(a7, x0, 1));
        asm.Add(pc => Inst.Ecall());
        asm.Add(pc => Inst.Addi(a0, 30, 0)); // Restore result

        // Continue Logic
        asm.Add(pc => Inst.Ld(ra, sp, 16));
        asm.Add(pc => Inst.Addi(sp, sp, 32));
        asm.Add(pc => Inst.Mul(a0, a0, t1));
        asm.Add(pc => Inst.Jalr(x0, ra, 0));

        // Base Case Return
        asm.Add(pc => Inst.Addi(a0, x0, 1), label: "base_case");
        asm.Add(pc => Inst.Ld(ra, sp, 16));
        asm.Add(pc => Inst.Addi(sp, sp, 32));
        asm.Add(pc => Inst.Jalr(x0, ra, 0));

        // Assemble & Load
        var insts = asm.Assemble();
        for(int i=0; i < insts.Count; i++) 
            state.Memory.WriteWord((uint)(i*4), InstructionEncoder.Encode(insts[i]));

        // --- DIAGNOSTIC DUMP ---
        // Read back the first instruction (0x00) to verify the JUMP offset
        uint firstInst = state.Memory.ReadWord(0x00);
        
        int cycles = 0;
        // Limit to 2000 cycles
        while (cycles < 2000 && state.ProgramCounter != 0x04)
        {
            pipeline.Cycle();
            cycles++;
        }

        string trace = string.Join(", ", host.IntOutputs);
        
        // Assert
        Assert.True(state.Registers.Read(10) == 120, 
            $"Failed. Hex@0x00: {firstInst:X8}. Trace: [{trace}]. Result: {state.Registers.Read(10)}");
    }
}
