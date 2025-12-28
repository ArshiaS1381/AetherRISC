using Xunit;
using AetherRISC.Tests.Infrastructure;
using AetherRISC.Core.Architecture.Hardware.ISA;

namespace AetherRISC.Tests.Unit.ISA.I;

public class Integer32BitTests : CpuTestFixture
{
    [Fact]
    public void Add_Overflow_Wraps_At_32Bits()
    {
        Init32();
        // 0xFFFFFFFF + 1 = 0x00000000 (in 32-bit)
        
        Assembler.Add(pc => Inst.Addi(1, 0, -1)); // 0xFFFFFFFF
        Assembler.Add(pc => Inst.Addi(2, 0, 1));
        Assembler.Add(pc => Inst.Add(3, 1, 2));
        
        Run(3);
        
        // In RV32, Registers are 64-bit stored but operations truncate to 32.
        // Or Registers are 32-bit. 
        // Based on your SystemConfig, XLEN=32 implies operations mask outputs.
        AssertReg(3, 0ul);
    }
}
