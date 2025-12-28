using AetherRISC.Core.Architecture.Hardware.Pipeline.Controller;

namespace AetherRISC.Core.Architecture.Hardware.Pipeline.Stages;

public interface IPipelineStage
{
    void Run(PipelineController cpu);
}
