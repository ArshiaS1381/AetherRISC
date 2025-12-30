using Xunit;
using AetherRISC.Tests.Infrastructure;
using AetherRISC.Core.Architecture.Simulation.Runners;
using AetherRISC.Core.Architecture.Hardware.Pipeline;
using AetherRISC.Core.Architecture.Hardware.Pipeline.Predictors;

namespace AetherRISC.Tests.Integration.Pipeline
{
    public class BranchPredictionTests : CpuTestFixture
    {
        [Fact]
        public void Static_Predictor_Always_NT()
        {
            Init64();
            var settings = new ArchitectureSettings { PipelineWidth = 1 };
            // Fully qualify NullLogger to resolve ambiguity
            var logger = new AetherRISC.Core.Helpers.NullLogger();
            
            var runner = new PipelinedRunner(Machine, logger, "static", settings);
            
            Assert.True(runner.Predictor is StaticPredictor);
            Assert.False(runner.Predictor.Predict(0x1000).PredictedTaken);
        }
    }
}
