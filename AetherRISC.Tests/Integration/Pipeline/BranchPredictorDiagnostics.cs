using Xunit;
using Xunit.Abstractions;
using AetherRISC.Tests.Infrastructure;
using AetherRISC.Core.Architecture.Simulation.Runners;
using AetherRISC.Core.Architecture.Hardware.Pipeline;
using AetherRISC.Core.Assembler;

namespace AetherRISC.Tests.Integration.Pipeline
{
    public class BranchPredictorDiagnostics : CpuTestFixture
    {
        private readonly ITestOutputHelper _output;
        public BranchPredictorDiagnostics(ITestOutputHelper output) => _output = output;

        [Theory]
        [InlineData("static")]
        [InlineData("bimodal-2bit")]
        [InlineData("gshare")]
        public void Benchmark_Predictor(string predictor)
        {
            Init64();
            var settings = new ArchitectureSettings { PipelineWidth = 1 };
            // Fully qualify NullLogger
            var logger = new AetherRISC.Core.Helpers.NullLogger();
            
            var runner = new PipelinedRunner(Machine, logger, predictor, settings);
            
            var source = @"
                li x1, 10
                loop:
                addi x1, x1, -1
                bnez x1, loop
                ebreak
            ";
            
            var asm = new SourceAssembler(source);
            asm.Assemble(Machine);
            
            for(int i=0; i<100; i++) runner.Step(i);
            
            _output.WriteLine($"Predictor {predictor}: {runner.Metrics.BranchAccuracy:F2}% Accuracy");
        }
    }
}
