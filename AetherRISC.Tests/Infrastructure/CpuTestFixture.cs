using Xunit;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Simulation.Runners;
using AetherRISC.Core.Architecture.Hardware.ISA.Encoding;
using AetherRISC.Core.Assembler;
using AetherRISC.Tests.Infrastructure;

namespace AetherRISC.Tests.Infrastructure
{
    public abstract class CpuTestFixture
    {
        public MachineState Machine { get; set; } = null!;
        public TestMemoryBus Memory { get; set; } = null!;
        public TestAssembler Assembler { get; set; } = null!;
        
        protected object Runner { get; set; } = null!;

        protected void Init64() => Init(64);
        protected void Init32() => Init(32);

        protected virtual void Init(int xlen)
        {
            var config = new SystemConfig(xlen, resetVector: 0x0000);
            Machine = new MachineState(config);
            
            Memory = new TestMemoryBus(1024 * 1024); 
            Machine.Memory = Memory;

            Assembler = new TestAssembler();
            Runner = new SimpleRunner(Machine, new Core.Helpers.NullLogger());
        }

        // Made public for integration tests
        public void Run(int cycles)
        {
            LoadToMemory();
            
            if (Runner is SimpleRunner s) s.Run(cycles);
            else if (Runner is PipelinedRunner p) 
            {
                for(int i=0; i<cycles; i++) p.Step(i);
            }
        }
        
        private void LoadToMemory()
        {
            var insts = Assembler.Assemble();
            uint addr = (uint)Machine.Config.ResetVector;
            foreach (var inst in insts)
            {
                uint raw = InstructionEncoder.Encode(inst);
                Memory.WriteWord(addr, raw);
                addr += 4;
            }
            Machine.Registers.PC = Machine.Config.ResetVector;
        }

        protected void AssertReg(int regIndex, ulong expected) => Assert.Equal(expected, Machine.Registers.Read(regIndex));
        protected void AssertReg(int regIndex, long expected) => Assert.Equal((ulong)expected, Machine.Registers.Read(regIndex));
        protected void AssertPC(ulong expected) => Assert.Equal(expected, Machine.Registers.PC);
    }
}
