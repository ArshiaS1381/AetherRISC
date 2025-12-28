using Xunit;
using AetherRISC.Tests.Infrastructure;
using AetherRISC.Core.Architecture.Hardware.ISA;

namespace AetherRISC.Tests.Unit.ISA.A;

public class AtomicTests : CpuTestFixture
{
    [Fact]
    public void Lr_Sc_Success_Loop()
    {
        Init64();
        Memory.WriteWord(0x100, 42);

        // 1. LR.W x1, (x2)  -> Load 42, Set Reservation on 0x100
        // 2. SC.W x3, x4, (x2) -> Write Success?
        
        Assembler.Add(pc => Inst.Addi(2, 0, 0x100)); // Addr
        Assembler.Add(pc => Inst.Addi(4, 0, 99));    // New Val
        
        Assembler.Add(pc => Inst.LrW(1, 2, 0));
        Assembler.Add(pc => Inst.ScW(3, 2, 4)); // x3 = status (0=success)

        Run(4);

        AssertReg(1, 42ul); // Loaded Value
        AssertReg(3, 0ul);  // Success status
        Assert.Equal(99u, Memory.ReadWord(0x100)); // Memory Updated
    }

    [Fact]
    public void Lr_Sc_Fail_AddressMismatch()
    {
        Init64();
        // Reservation on 0x100
        // Store to 0x200
        
        Assembler.Add(pc => Inst.Addi(2, 0, 0x100)); 
        Assembler.Add(pc => Inst.Addi(5, 0, 0x200)); 
        
        Assembler.Add(pc => Inst.LrW(1, 2, 0));         // Resv @ 0x100
        Assembler.Add(pc => Inst.ScW(3, 5, 1));      // Store @ 0x200 (Should Fail?)
        // Note: Real HW might succeed if cache lines overlap, but simple sim usually checks exact addr.
        // AetherRISC simple model requires exact match.
        
        Run(4);
        
        AssertReg(3, 1ul); // 1 = Failure
    }

    [Fact]
    public void AmoAdd_AtomicAddition()
    {
        Init64();
        Memory.WriteWord(0x100, 10);
        
        Assembler.Add(pc => Inst.Addi(1, 0, 0x100));
        Assembler.Add(pc => Inst.Addi(2, 0, 5));
        // AMOADD.W x3, x2, (x1) -> x3 = old(10), Mem = 10+5=15
        Assembler.Add(pc => Inst.AmoAddW(3, 1, 2));
        
        Run(3);
        
        AssertReg(3, 10ul);
        Assert.Equal(15u, Memory.ReadWord(0x100));
    }
}

