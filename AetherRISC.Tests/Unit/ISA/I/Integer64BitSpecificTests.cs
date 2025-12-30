using Xunit;
using AetherRISC.Tests.Infrastructure;
using AetherRISC.Core.Architecture.Hardware.ISA;

namespace AetherRISC.Tests.Unit.ISA.I
{
    public class Integer64BitSpecificTests : CpuTestFixture
    {
        [Fact]
        public void Addiw_SignExtends_Result()
        {
            Init64();
            // ADDIW x1, x0, -1  => x1 = 0xFFFFFFFF (32-bit -1) -> sign extended to 64-bit -1 (0xFFFFFFFFFFFFFFFF)
            
            // Note: Manual injection because asm assumes LI handles width
            Assembler.Add(pc => Inst.Addiw(1, 0, -1));
            
            base.Run(5);
            
            AssertReg(1, 0xFFFFFFFFFFFFFFFFUL);
        }

        [Fact]
        public void Addw_Truncates_And_SignExtends()
        {
            Init64();
            // x2 = 0x00000001_80000000
            // x3 = 0x00000000_80000000
            // ADDW x1, x2, x3
            // Lower 32: 0x80000000 + 0x80000000 = 0x00000000 (overflow 32-bit)
            // Sign extend 0 -> 0
            
            Machine.Registers.Write(2, 0x180000000UL);
            Machine.Registers.Write(3, 0x080000000UL);
            
            Assembler.Add(pc => Inst.Addw(1, 2, 3));
            
            base.Run(5);
            
            AssertReg(1, 0UL);
        }
    }
}
