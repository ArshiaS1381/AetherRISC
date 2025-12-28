using AetherRISC.Core.Architecture.Hardware.Pipeline;

namespace AetherRISC.Core.Architecture.Hardware.Pipeline.Hazards;

public interface IHazardUnit
{
    // Now accepts Buffers directly
    bool DetectAndHandle(PipelineBuffers buffers);
}
