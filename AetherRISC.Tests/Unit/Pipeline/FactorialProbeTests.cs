using Xunit;
using Xunit.Abstractions;
using AetherRISC.Tests.Infrastructure;
using AetherRISC.Core.Assembler;

namespace AetherRISC.Tests.Unit.Pipeline
{
    public class FactorialProbeTests : PipelineTestFixture
    {
        private readonly ITestOutputHelper _output;
        public FactorialProbeTests(ITestOutputHelper output) => _output = output;

        [Fact]
        public void Probe_Factorial_Hazard()
        {
            var source = @"
                li x1, 5
                li x2, 1
                mul x2, x2, x1
                nop
            ";
            
            InitPipeline(1);
            var asm = new SourceAssembler(source);
            asm.Assemble(Machine);
            Machine.Registers.PC = asm.TextBase;

            // LI x1
            Pipeline.Step(0); 
            // LI x2
            Pipeline.Step(1); 
            // MUL x2, x2, x1 (Hazard on x1 and x2)
            Pipeline.Step(2); 
            
            var ex = Pipeline.PipelineState.ExecuteMemory.Slots[0];
            
            // In superscalar, logic is inside the slots
            if (ex.Valid && ex.DecodedInst?.Mnemonic == "MUL")
            {
                _output.WriteLine($"MUL Result: {ex.AluResult}");
                Assert.Equal(5ul, ex.AluResult);
            }
        }
    }
}
