using AetherRISC.Core.Architecture.Hardware.Pipeline;
using AetherRISC.Core.Architecture.Hardware.ISA;

namespace AetherRISC.Core.Architecture.Hardware.Pipeline.Hazards;

public class ControlHazardUnit : IHazardUnit
{
    public bool DetectAndHandle(PipelineBuffers buffers)
    {
        // Only check valid buffers to avoid reading stale "Ghost" flags from bubbles
        if (!buffers.ExecuteMemory.IsEmpty)
        {
            if (buffers.ExecuteMemory.BranchTaken)
            {
                buffers.DecodeExecute.Flush();
                return true;
            }
        }

        return false;
    }
}
