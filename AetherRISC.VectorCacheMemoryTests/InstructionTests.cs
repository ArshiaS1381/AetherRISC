using Xunit;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture;
using AetherRISC.Core.Architecture.Hardware.Memory;
using AetherRISC.Core.Assembler;
using AetherRISC.Core.Architecture.Simulation.Runners;

namespace AetherRISC.VectorCacheMemoryTests
{
    public class InstructionTests
    {
        private MachineState Setup(string asm)
        {
            var s = new MachineState(SystemConfig.Rv64(), new ArchitectureSettings());
            s.AttachMemory(new SystemBus(0xFFFFFFFF));
            new SourceAssembler(asm).Assemble(s);
            return s;
        }

        private void Run(MachineState s, int steps = 1) => new SimpleRunner(s).Step(steps);

        [Fact] public void Op_ADD() {
            var s = Setup("li x1, 10\nli x2, 20\nadd x3, x1, x2");
            Run(s, 3);
            Assert.Equal(30ul, s.Registers.Read(3));
        }

        [Fact] public void Op_SUB() {
            var s = Setup("li x1, 10\nli x2, 3\nsub x3, x1, x2");
            Run(s, 3);
            Assert.Equal(7ul, s.Registers.Read(3));
        }

        [Fact] public void Op_SLL() {
            var s = Setup("li x1, 1\nslli x2, x1, 4");
            Run(s, 2);
            Assert.Equal(16ul, s.Registers.Read(2));
        }

        [Fact] public void Op_XOR() {
            var s = Setup("li x1, 5\nli x2, 3\nxor x3, x1, x2");
            Run(s, 3);
            Assert.Equal(6ul, s.Registers.Read(3)); 
        }

        [Fact] public void Op_SLT_Signed() {
            var s = Setup("li x1, -1\nli x2, 1\nslt x3, x1, x2");
            Run(s, 3);
            Assert.Equal(1ul, s.Registers.Read(3)); 
        }

        [Fact] public void Op_SLTU_Unsigned() {
            var s = Setup("li x1, -1\nli x2, 1\nsltu x3, x1, x2");
            Run(s, 3);
            Assert.Equal(0ul, s.Registers.Read(3)); 
        }

        [Fact] public void Op_LUI() {
            var s = Setup("lui x1, 0x1");
            Run(s, 1);
            Assert.Equal(0x1000ul, s.Registers.Read(1));
        }

        [Fact] public void Op_AUIPC() {
            var s = Setup("auipc x1, 0x1"); 
            Run(s, 1);
            // PC base is 0x00400000. 0x1 << 12 = 0x1000. Sum = 0x401000.
            Assert.Equal(0x401000ul, s.Registers.Read(1));
        }

        [Fact] public void Op_JAL() {
            // jal x1, target (1)
            // li x2, 1 (skipped)
            // target: li x2, 2 (1)
            var s = Setup("jal x1, target\nli x2, 1\ntarget:\nli x2, 2");
            Run(s, 2); 
            Assert.Equal(2ul, s.Registers.Read(2));
            Assert.Equal(0x400004ul, s.Registers.Read(1)); 
        }

        [Fact] public void Op_BEQ_Taken() {
            // li (1), li (1), beq (1), li (1) = 4
            var s = Setup("li x1, 1\nli x2, 1\nbeq x1, x2, target\nli x3, 0\ntarget:\nli x3, 1");
            Run(s, 4); 
            Assert.Equal(1ul, s.Registers.Read(3));
        }

        [Fact] public void Op_BEQ_NotTaken() {
            // li (1), li (1), beq (1), li (1) = 4
            var s = Setup("li x1, 1\nli x2, 2\nbeq x1, x2, target\nli x3, 5\nebreak\ntarget:\nli x3, 1");
            Run(s, 4);
            Assert.Equal(5ul, s.Registers.Read(3));
        }

        [Fact] public void Mem_StoreLoad_Byte() {
            // li large (2), li (1), sb (1), lb (1) = 5
            var s = Setup("li x1, 0x1000\nli x2, 0xFF\nsb x2, 0(x1)\nlb x3, 0(x1)");
            Run(s, 5);
            Assert.Equal(0xFFFFFFFFFFFFFFFFul, s.Registers.Read(3));
        }

        [Fact] public void Mem_StoreLoad_ByteUnsigned() {
            // li large (2), li (1), sb (1), lbu (1) = 5
            var s = Setup("li x1, 0x1000\nli x2, 0xFF\nsb x2, 0(x1)\nlbu x3, 0(x1)");
            Run(s, 5);
            Assert.Equal(0xFFul, s.Registers.Read(3));
        }

        [Fact] public void Mem_StoreLoad_Double() {
            // li large (2), li (1), sd (1), ld (1) = 5
            var s = Setup("li x1, 0x1000\nli x2, -1\nsd x2, 0(x1)\nld x3, 0(x1)");
            Run(s, 5);
            Assert.Equal(0xFFFFFFFFFFFFFFFFul, s.Registers.Read(3));
        }

        [Fact] public void Op_ADDW_Truncates() {
            // li large (2), addw (1) = 3
            var s = Setup("li x1, 0xFFFFFFFFFFFFFFFF\naddw x2, x1, x1");
            Run(s, 3);
            Assert.Equal(0xFFFFFFFFFFFFFFFEul, s.Registers.Read(2));
        }

        [Fact] public void Sys_EBREAK_Halts() {
            var s = Setup("ebreak\nli x1, 1");
            new SimpleRunner(s).Run();
            Assert.True(s.Halted);
            Assert.Equal(0ul, s.Registers.Read(1));
        }

        [Fact] public void Csr_ReadWrite() {
            // li large (2), csrrw (1), csrr (1) = 4
            var s = Setup("li x1, 0xDEAD\ncsrrw x2, 0x340, x1\ncsrr x3, 0x340");
            Run(s, 4);
            Assert.Equal(0xDEADul, s.Csr.Read(0x340));
            Assert.Equal(0xDEADul, s.Registers.Read(3));
        }
    }
}
