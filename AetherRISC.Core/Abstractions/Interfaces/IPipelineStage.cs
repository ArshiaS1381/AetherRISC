using AetherRISC.Core.Architecture;
using AetherRISC.Core.Architecture.Pipeline;

namespace AetherRISC.Core.Abstractions.Interfaces;

public interface IPipelineStage
{
    /// <summary>
    /// Executes one clock cycle for this stage.
    /// </summary>
    /// <param name="state">Global CPU state.</param>
    /// <param name="inLatch">Data coming from the previous stage.</param>
    /// <param name="outLatch">Data to pass to the next stage.</param>
    void Tick(MachineState state, PipelineLatch inLatch, PipelineLatch outLatch);

    /// <summary>
    /// Clears the stage state (used for Pipeline Flushes/Hazards).
    /// </summary>
    void Reset();
}
