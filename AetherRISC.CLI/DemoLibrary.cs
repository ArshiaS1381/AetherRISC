/*
 * Project:     AetherRISC
 * File:        DemoLibrary.cs
 * Version:     2.1.0
 * Description: Added LUI Test.
 */

using AetherRISC.CLI;
using AetherRISC.Core.Helpers;

public static class DemoLibrary
{
    const int x0=0, ra=1, sp=2, a0=10, a1=11, a2=12, a7=17, t0=5, t1=6, t2=7;

    public static void LoadLuiTest(LabelAssembler asm)
    {
        // TARGET: Load 0x12345000 into t0
        // LUI instruction usually takes the upper 20 bits shifted.
        // Our Helper Inst.Lui takes the *Raw 32-bit Value* we want to end up with.
        
        asm.Add(pc => Inst.Lui(t0, 0x12345000)); 
        
        // Print result (Should be 305418240 decimal)
        asm.Add(pc => Inst.Addi(a0, t0, 0));
        asm.Add(pc => Inst.Addi(a7, x0, 1));
        asm.Add(pc => Inst.Ecall());

        // Test Negative (Sign Extension check)
        // Load 0xFFFFF000 (which is -4096 in 32-bit)
        // In RV64, this should sign-extend to 0xFFFFFFFFFFFFF000.
        asm.Add(pc => Inst.Lui(t1, unchecked((int)0xFFFFF000)));
        
        asm.Add(pc => Inst.Addi(a0, t1, 0));
        asm.Add(pc => Inst.Addi(a7, x0, 1));
        asm.Add(pc => Inst.Ecall());
    }

    public static void LoadOverflowTest(LabelAssembler asm)
    {
        asm.Add(pc => Inst.Addi(t0, x0, -1)); 
        asm.Add(pc => Inst.Addi(t1, t0, 1)); 
        
        asm.Add(pc => Inst.Lui(t0, 0x80000));     
        asm.Add(pc => Inst.Addi(t0, t0, -1));     
        asm.Add(pc => Inst.Addi(t1, t0, 1));      
        
        asm.Add(pc => Inst.Addi(a0, t0, 0));
        asm.Add(pc => Inst.Addi(a7, x0, 1));
        asm.Add(pc => Inst.Ecall());

        asm.Add(pc => Inst.Addi(a0, t1, 0));
        asm.Add(pc => Inst.Addi(a7, x0, 1));
        asm.Add(pc => Inst.Ecall());
    }

    public static void LoadArraySum32(LabelAssembler asm, int baseAddr, int count)
    {
        asm.Add(pc => Inst.Addi(a0, x0, 0));          
        asm.Add(pc => Inst.Addi(a1, x0, baseAddr));   
        asm.Add(pc => Inst.Addi(a2, x0, count));      

        asm.Add(pc => Inst.Bne(a2, x0, asm.To("loop", pc)), label: "check");
        asm.Add(pc => Inst.Jal(x0, asm.To("done", pc)));

        asm.Add(pc => Inst.Lw(t0, a1, 0), label: "loop"); 
        asm.Add(pc => Inst.Add(a0, a0, t0));              
        asm.Add(pc => Inst.Addi(a1, a1, 4));              
        asm.Add(pc => Inst.Addi(a2, a2, -1));             
        asm.Add(pc => Inst.Jal(x0, asm.To("check", pc)));

        asm.Add(pc => Inst.Addi(a7, x0, 1), label: "done"); 
        asm.Add(pc => Inst.Ecall());
    }

    public static void LoadArraySum64(LabelAssembler asm, int baseAddr, int count)
    {
        asm.Add(pc => Inst.Addi(a0, x0, 0));          
        asm.Add(pc => Inst.Addi(a1, x0, baseAddr));   
        asm.Add(pc => Inst.Addi(a2, x0, count));      

        asm.Add(pc => Inst.Bne(a2, x0, asm.To("loop", pc)), label: "check");
        asm.Add(pc => Inst.Jal(x0, asm.To("done", pc)));

        asm.Add(pc => Inst.Ld(t0, a1, 0), label: "loop"); 
        asm.Add(pc => Inst.Add(a0, a0, t0));              
        asm.Add(pc => Inst.Addi(a1, a1, 8));          
        asm.Add(pc => Inst.Addi(a2, a2, -1));             
        asm.Add(pc => Inst.Jal(x0, asm.To("check", pc)));

        asm.Add(pc => Inst.Addi(a7, x0, 1), label: "done"); 
        asm.Add(pc => Inst.Ecall());
    }

    public static void LoadFibonacci(LabelAssembler asm, int count)
    {
        asm.Add(pc => Inst.Addi(t0, x0, 0));      
        asm.Add(pc => Inst.Addi(t1, x0, 1));      
        asm.Add(pc => Inst.Addi(t2, x0, count));  

        asm.Add(pc => Inst.Addi(a0, t0, 0));
        asm.Add(pc => Inst.Addi(a7, x0, 1));
        asm.Add(pc => Inst.Ecall());

        asm.Add(pc => Inst.Bne(t2, x0, asm.To("loop", pc)));
        asm.Add(pc => Inst.Jal(x0, asm.To("exit", pc)));

        asm.Add(pc => Inst.Addi(a0, t1, 0), label: "loop");
        asm.Add(pc => Inst.Addi(a7, x0, 1));      
        asm.Add(pc => Inst.Ecall());
        
        asm.Add(pc => Inst.Add(a2, t0, t1));      
        asm.Add(pc => Inst.Addi(t0, t1, 0));      
        asm.Add(pc => Inst.Addi(t1, a2, 0));      
        
        asm.Add(pc => Inst.Addi(t2, t2, -1));
        asm.Add(pc => Inst.Bne(t2, x0, asm.To("loop", pc)));

        asm.Add(pc => Inst.Nop(), label: "exit");
    }

    public static void LoadGCD(LabelAssembler asm, int a, int b)
    {
        asm.Add(pc => Inst.Addi(a0, x0, a));
        asm.Add(pc => Inst.Addi(a1, x0, b));

        asm.Add(pc => Inst.Bne(a0, a1, asm.To("check", pc)), label: "loop");
        asm.Add(pc => Inst.Jal(x0, asm.To("done", pc)));

        asm.Add(pc => Inst.Slt(t0, a1, a0), label: "check");
        asm.Add(pc => Inst.Bne(t0, x0, asm.To("a_greater", pc))); 

        asm.Add(pc => Inst.Sub(a1, a1, a0));
        asm.Add(pc => Inst.Jal(x0, asm.To("loop", pc)));

        asm.Add(pc => Inst.Sub(a0, a0, a1), label: "a_greater");
        asm.Add(pc => Inst.Jal(x0, asm.To("loop", pc)));

        asm.Add(pc => Inst.Addi(a7, x0, 1), label: "done");
        asm.Add(pc => Inst.Ecall());
    }

    public static void LoadFactorial(LabelAssembler asm, int n)
    {
        asm.Add(pc => Inst.Addi(sp, x0, 1024));       
        asm.Add(pc => Inst.Addi(sp, sp, 1024)); 
        asm.Add(pc => Inst.Addi(a0, x0, n));          
        asm.Add(pc => Inst.Jal(ra, asm.To("fact", pc))); 
        asm.Add(pc => Inst.Jal(x0, asm.To("exit", pc))); 

        asm.Add(pc => Inst.Addi(sp, sp, -32), label: "fact");
        asm.Add(pc => Inst.Sd(sp, ra, 16));
        asm.Add(pc => Inst.Sd(sp, a0, 8));

        asm.Add(pc => Inst.Addi(t0, x0, 2));
        asm.Add(pc => Inst.Slt(t0, a0, t0));           
        asm.Add(pc => Inst.Bne(t0, x0, asm.To("base_case", pc))); 

        asm.Add(pc => Inst.Addi(a0, a0, -1));          
        asm.Add(pc => Inst.Jal(ra, asm.To("fact", pc))); 

        asm.Add(pc => Inst.Ld(t1, sp, 8));             
        asm.Add(pc => Inst.Ld(ra, sp, 16));            
        asm.Add(pc => Inst.Addi(sp, sp, 32));          
        asm.Add(pc => Inst.Mul(a0, a0, t1));           
        asm.Add(pc => Inst.Jalr(x0, ra, 0));           

        asm.Add(pc => Inst.Addi(a0, x0, 1), label: "base_case"); 
        asm.Add(pc => Inst.Addi(sp, sp, 32));          
        asm.Add(pc => Inst.Jalr(x0, ra, 0));           

        asm.Add(pc => Inst.Addi(a0, a0, 0), label: "exit");
        asm.Add(pc => Inst.Addi(a7, x0, 1));
        asm.Add(pc => Inst.Ecall());
    }
}
