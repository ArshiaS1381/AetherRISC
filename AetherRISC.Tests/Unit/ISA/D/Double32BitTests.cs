using Xunit;
using AetherRISC.Tests.Infrastructure;
using AetherRISC.Core.Architecture.Hardware.ISA;

namespace AetherRISC.Tests.Unit.ISA.D;

public class Double32BitTests : CpuTestFixture
{
    [Fact]
    public void Fld_Fsd_Work_In_32Bit_Mode()
    {
        // RV32D allows 64-bit floats even if integer regs are 32-bit
        Init32();
        
        // Load address 0x100
        Assembler.Add(pc => Inst.Addi(1, 0, 0x100));
        
        // Load PI from memory into F1
        Memory.WriteDouble(0x100, 0x400921FB54442D18);
        Assembler.Add(pc => Inst.Fld(1, 1, 0));
        
        Run(2);
        
        Assert.Equal(3.141592653589793, Machine.FRegisters.ReadDouble(1));
    }
}
