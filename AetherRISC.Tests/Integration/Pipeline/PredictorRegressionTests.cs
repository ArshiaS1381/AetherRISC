using Xunit;
using AetherRISC.Tests.Infrastructure;
using AetherRISC.Core.Architecture.Simulation.Runners;
using AetherRISC.Core.Architecture.Hardware.Pipeline;
using AetherRISC.Core.Architecture.Hardware.Pipeline.Predictors;

namespace AetherRISC.Tests.Integration.Pipeline
{
    public class PredictorRegressionTests : CpuTestFixture
    {
        [Fact]
        public void Bimodal_Initializes_Correctly()
        {
            Init64();
            var settings = new ArchitectureSettings { PipelineWidth = 1, BranchPredictorInitialValue = 2 };
            // Fully qualify NullLogger
            var logger = new AetherRISC.Core.Helpers.NullLogger();
            
            var runner = new PipelinedRunner(Machine, logger, "bimodal", settings);
            
            Assert.True(runner.Predictor is Bimodal2bPredictor);
        }
    }
}
