using Xunit;
using AetherRISC.Tests.Infrastructure;
using AetherRISC.Core.Assembler;

namespace AetherRISC.Tests.Integration.Assembler
{
    public class AssemblerDirectivesTests : CpuTestFixture
    {
        [Fact]
        public void Asciz_Stores_Null_Terminated_String()
        {
            Init64();
            var source = @"
                .data
                str: .asciz ""Hello""
            ";
            
            var asm = new SourceAssembler(source);
            asm.Assemble(Machine);
            
            // H e l l o \0
            Assert.Equal((byte)'H', Machine.Memory.ReadByte(asm.DataBase));
            Assert.Equal((byte)'e', Machine.Memory.ReadByte(asm.DataBase + 1));
            Assert.Equal((byte)'l', Machine.Memory.ReadByte(asm.DataBase + 2));
            Assert.Equal((byte)'l', Machine.Memory.ReadByte(asm.DataBase + 3));
            Assert.Equal((byte)'o', Machine.Memory.ReadByte(asm.DataBase + 4));
            Assert.Equal(0, Machine.Memory.ReadByte(asm.DataBase + 5));
        }

        [Fact]
        public void Space_Allocates_Zeroed_Memory()
        {
            Init64();
            var source = @"
                .data
                .byte 0xFF
                .space 4
                .byte 0xEE
            ";
            
            var asm = new SourceAssembler(source);
            asm.Assemble(Machine);
            
            // 0: FF
            // 1: 00
            // 2: 00
            // 3: 00
            // 4: 00
            // 5: EE
            
            Assert.Equal(0xFF, Machine.Memory.ReadByte(asm.DataBase));
            Assert.Equal(0, Machine.Memory.ReadWord(asm.DataBase + 1));
            Assert.Equal(0xEE, Machine.Memory.ReadByte(asm.DataBase + 5));
        }

        [Fact]
        public void Equ_Defines_Constant()
        {
            Init64();
            var source = @"
                .equ VAL, 42
                .text
                li x1, VAL
            ";
            
            var asm = new SourceAssembler(source);
            asm.Assemble(Machine);
            
            base.Run(5);
            AssertReg(1, 42);
        }
    }
}
