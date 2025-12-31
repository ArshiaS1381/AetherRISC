using Xunit;
using AetherRISC.Core.Assembler;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture;
using AetherRISC.Core.Architecture.Hardware.Memory;

namespace AetherRISC.VectorCacheMemoryTests
{
    public class AssemblerTests
    {
        private MachineState GetState()
        {
            var s = new MachineState(SystemConfig.Rv64(), new ArchitectureSettings());
            s.AttachMemory(new SystemBus(0xFFFFFFFF)); 
            return s;
        }

        [Fact] public void Directive_Byte() {
            var s = GetState();
            new SourceAssembler(".data\n.byte 10, 0xFF").Assemble(s);
            Assert.Equal(10, s.Memory.ReadByte(0x10010000));
            Assert.Equal(255, s.Memory.ReadByte(0x10010001));
        }

        [Fact] public void Directive_Half() {
            var s = GetState();
            new SourceAssembler(".data\n.half 0x1234").Assemble(s);
            Assert.Equal(0x1234, s.Memory.ReadHalf(0x10010000));
        }

        [Fact] public void Directive_Word() {
            var s = GetState();
            new SourceAssembler(".data\n.word 0xDEADBEEF").Assemble(s);
            Assert.Equal(0xDEADBEEFu, s.Memory.ReadWord(0x10010000));
        }

        [Fact] public void Directive_Dword() {
            var s = GetState();
            new SourceAssembler(".data\n.dword 0x1122334455667788").Assemble(s);
            Assert.Equal(0x1122334455667788ul, s.Memory.ReadDouble(0x10010000));
        }

        [Fact] public void Directive_Asciz() {
            var s = GetState();
            new SourceAssembler(".data\n.asciz \"Hello\"").Assemble(s);
            Assert.Equal('H', (char)s.Memory.ReadByte(0x10010000));
            Assert.Equal(0, s.Memory.ReadByte(0x10010005));
        }

        [Fact] public void Directive_Zero_Space() {
            var s = GetState();
            new SourceAssembler(".data\n.byte 1\n.zero 3\n.byte 2").Assemble(s);
            Assert.Equal(1, s.Memory.ReadByte(0x10010000));
            Assert.Equal(0, s.Memory.ReadByte(0x10010001));
            Assert.Equal(2, s.Memory.ReadByte(0x10010004));
        }

        [Fact] public void Directive_Align() {
            var s = GetState();
            new SourceAssembler(".data\n.byte 1\n.align 3\n.byte 2").Assemble(s);
            Assert.Equal(1, s.Memory.ReadByte(0x10010000));
            Assert.Equal(2, s.Memory.ReadByte(0x10010008));
        }

        [Fact] public void Directive_Equ() {
            var s = GetState();
            new SourceAssembler(".equ VAL, 100\n.text\nli x1, VAL").Assemble(s);
            new AetherRISC.Core.Architecture.Simulation.Runners.SimpleRunner(s).Step(1);
            Assert.Equal(100ul, s.Registers.Read(1));
        }

        [Fact] public void Label_Reference_Forward() {
            var s = GetState();
            string asm = @"
                .text
                j target
                li x1, 1
                target:
                li x1, 2
            ";
            new SourceAssembler(asm).Assemble(s);
            uint inst = s.Memory.ReadWord(0x00400000); 
            Assert.NotEqual(0u, inst);
        }

        [Fact] public void Pseudo_LI_Big() {
            var s = GetState();
            new SourceAssembler("li x1, 0x12345678").Assemble(s);
            var runner = new AetherRISC.Core.Architecture.Simulation.Runners.SimpleRunner(s);
            runner.Step(2);
            Assert.Equal(0x12345678ul, s.Registers.Read(1));
        }

        [Fact] public void Pseudo_MV() {
            var s = GetState();
            s.Registers.Write(2, 55);
            new SourceAssembler("mv x1, x2").Assemble(s);
            new AetherRISC.Core.Architecture.Simulation.Runners.SimpleRunner(s).Step(1);
            Assert.Equal(55ul, s.Registers.Read(1));
        }

        [Fact] public void Pseudo_NOP() {
            var s = GetState();
            new SourceAssembler("nop").Assemble(s);
            uint inst = s.Memory.ReadWord(0x00400000);
            Assert.Equal(0x00000013u, inst); 
        }
    }
}
