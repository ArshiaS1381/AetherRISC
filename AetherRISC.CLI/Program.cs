using System;
using System.Collections.Generic;
using AetherRISC.Core.Architecture;
using AetherRISC.Core.Architecture.Memory;
using AetherRISC.Core.Architecture.Pipeline;
using AetherRISC.Core.Hardware.ISA.Encoding;
using AetherRISC.Core.Helpers;
using AetherRISC.CLI;
using AetherRISC.Core.Hardware.ISA.Base;

var config = SystemConfig.Rv64();
var state = new MachineState(config);
state.Memory = new SystemBus(4096);
var host = new ConsoleHost();
state.Host = host;
var pipeline = new PipelineController(state);

const int x0=0, ra=1, sp=2, a0=10, a7=17, t0=5, t1=6;
var asm = new LabelAssembler();

// --- ADDRESS 0: THE FORCED BOOTSTRAP ---
asm.Add(pc => Inst.Jal(x0, asm.To("main_entry", pc))); 

// --- EXIT BLOCK (Placed safely away from address 0) ---
asm.Add(pc => Inst.Addi(a7, x0, 1), label: "halt_and_print"); 
asm.Add(pc => Inst.Ecall());                  
asm.Add(pc => Inst.Addi(a7, x0, 10));        
asm.Add(pc => Inst.Ecall());                  

// --- MAIN ENTRY ---
asm.Add(pc => Inst.Addi(sp, x0, 2048), label: "main_entry");       
asm.Add(pc => Inst.Addi(a0, x0, 5));          
asm.Add(pc => Inst.Jal(ra, asm.To("fact", pc))); 
asm.Add(pc => Inst.Jal(x0, asm.To("halt_and_print", pc))); // Go print result when done

// --- RECURSIVE FACTORIAL ---
asm.Add(pc => Inst.Addi(sp, sp, -32), label: "fact");
asm.Add(pc => Inst.Sd(ra, sp, 16));
asm.Add(pc => Inst.Sd(a0, sp, 8));

asm.Add(pc => Inst.Addi(t0, x0, 2));
asm.Add(pc => Inst.Slt(t0, a0, t0));
asm.Add(pc => Inst.Bne(t0, x0, asm.To("base", pc)));

asm.Add(pc => Inst.Addi(a0, a0, -1));
asm.Add(pc => Inst.Jal(ra, asm.To("fact", pc)));

asm.Add(pc => Inst.Ld(t1, sp, 8));            
asm.Add(pc => Inst.Ld(ra, sp, 16));           
asm.Add(pc => Inst.Addi(sp, sp, 32));         
asm.Add(pc => Inst.Mul(a0, a0, t1));          
asm.Add(pc => Inst.Jalr(x0, ra, 0));          

asm.Add(pc => Inst.Addi(a0, x0, 1), label: "base");
asm.Add(pc => Inst.Ld(ra, sp, 16));
asm.Add(pc => Inst.Addi(sp, sp, 32));
asm.Add(pc => Inst.Jalr(x0, ra, 0));

var insts = asm.Assemble();
for(int i=0; i < insts.Count; i++) 
    state.Memory.WriteWord((uint)(i*4), InstructionEncoder.Encode(insts[i]));

long cycles = 0;
while (host.IsRunning) {
    pipeline.Cycle();
    cycles++;
    Visualizer.RenderPipeline(pipeline, state, cycles);
}
