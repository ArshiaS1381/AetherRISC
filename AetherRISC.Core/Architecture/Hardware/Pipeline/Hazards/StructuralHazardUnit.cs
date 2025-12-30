using AetherRISC.Core.Architecture.Hardware.Pipeline;

namespace AetherRISC.Core.Architecture.Hardware.Pipeline.Hazards
{
    public class StructuralHazardUnit : IHazardUnit
    {
        public bool DetectAndHandle(PipelineBuffers buffers)
        {
            // Structural hazards (resource conflicts) logic would go here.
            // Currently unused as we model infinite functional units.
            // Load-Use is handled in DataHazardUnit now.
            return false;
        }
    }
}
