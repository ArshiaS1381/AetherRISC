using Xunit;
using AetherRISC.Tests.Infrastructure; // Required for PipelineTestFixture
using AetherRISC.Core.Architecture.Hardware.ISA;

namespace AetherRISC.Tests.Unit.Pipeline
{
    public class HaltBehaviorTests : PipelineTestFixture
    {
        [Fact]
        public void Ebreak_Halts_Simulation_Immediately()
        {
            InitPipeline(1); // 1-wide scalar for simple halt test

            // 1. ADDI x1, x0, 10
            // 2. EBREAK
            // 3. ADDI x1, x1, 1 (Should not execute)
            
            Assembler.Add(pc => Inst.Addi(1, 0, 10));
            Assembler.Add(pc => Inst.Ebreak(0, 0, 1));
            Assembler.Add(pc => Inst.Addi(1, 1, 1));

            LoadProgram();

            // Run enough cycles to fetch/decode/execute
            Cycle(10);

            Assert.True(Machine.Halted, "Machine should be halted after EBREAK");
            
            // Validate x1 is 10, meaning the instruction AFTER ebreak didn't commit
            AssertReg(1, 10); 
        }
    }
}
