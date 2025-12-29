using AetherRISC.Core.Architecture.Hardware.Pipeline;

namespace AetherRISC.Core.Architecture.Hardware.Pipeline.Hazards;

public class ControlHazardUnit : IHazardUnit
{
    public bool DetectAndHandle(PipelineBuffers buffers)
    {
        // Only check valid buffers
        if (!buffers.ExecuteMemory.IsEmpty)
        {
            // NEW LOGIC: Only flush if we MISPREDICTED.
            // If we predicted correctly, we keep streaming (Seamless!)
            
            if (buffers.ExecuteMemory.Misprediction)
            {
                // 1. Flush younger stages (Decode and Fetch are processing wrong path instructions)
                buffers.DecodeExecute.Flush();
                buffers.FetchDecode.Flush();
                
                // 2. We do NOT need to reset PC here, because ExecuteStage already
                // corrected the _state.Registers.PC to the "CorrectTarget".
                // The next Fetch cycle will pick up from the correct location.
                
                return true;
            }
        }

        return false;
    }
}
