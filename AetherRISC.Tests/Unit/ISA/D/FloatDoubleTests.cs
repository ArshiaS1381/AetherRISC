using Xunit;
using AetherRISC.Tests.Infrastructure;
using AetherRISC.Core.Architecture.Hardware.ISA;

namespace AetherRISC.Tests.Unit.ISA.D;

public class FloatDoubleTests : CpuTestFixture
{
    [Fact]
    public void Fsub_D_Precision()
    {
        Init64();
        // We need to load doubles. Hard via immediate.
        // Use Integer Load -> FMV.D.X
        
        ulong piBits = 0x400921FB54442D18; // 3.141592653589793
        Memory.WriteDouble(0x100, piBits);
        
        Assembler.Add(pc => Inst.Addi(1, 0, 0x100));
        Assembler.Add(pc => Inst.Fld(1, 1, 0)); // f1 = PI
        Assembler.Add(pc => Inst.FaddD(2, 1, 1)); // f2 = 2*PI
        
        Run(3);
        
        Assert.Equal(3.141592653589793 * 2, Machine.FRegisters.ReadDouble(2));
    }

    [Fact]
    public void Fclass_D_Infinity()
    {
        Init64();
        ulong infBits = 0x7FF0000000000000; // +Infinity
        Memory.WriteDouble(0x100, infBits);
        
        Assembler.Add(pc => Inst.Addi(1, 0, 0x100));
        Assembler.Add(pc => Inst.Fld(1, 1, 0)); 
        Assembler.Add(pc => Inst.FclassD(2, 1, 0)); 
        
        Run(3);
        
        // Bit 7 is +Infinity
        AssertReg(2, 1ul << 7);
    }
}

