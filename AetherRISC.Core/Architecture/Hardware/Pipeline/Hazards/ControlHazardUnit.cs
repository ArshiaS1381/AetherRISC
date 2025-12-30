using AetherRISC.Core.Architecture.Hardware.Pipeline;

namespace AetherRISC.Core.Architecture.Hardware.Pipeline.Hazards
{
    public class ControlHazardUnit : IHazardUnit
    {
        public bool DetectAndHandle(PipelineBuffers buffers)
        {
            if (buffers.ExecuteMemory.IsEmpty) return false;

            bool flushNeeded = false;
            int branchSlotIndex = -1;

            // 1. Scan for mispredictions
            for(int i=0; i<buffers.ExecuteMemory.Slots.Length; i++)
            {
                var slot = buffers.ExecuteMemory.Slots[i];
                if (slot.Valid && slot.Misprediction)
                {
                    flushNeeded = true;
                    branchSlotIndex = i;
                    // Note: ExecuteStage already repaired global PC
                    break; 
                }
            }

            if (flushNeeded)
            {
                // [FIX] Kill Shadow Instructions in the SAME Execute bundle
                // If Branch is at Slot 0, Slots 1,2,3 are on the wrong path and must die.
                for (int i = branchSlotIndex + 1; i < buffers.ExecuteMemory.Slots.Length; i++)
                {
                    buffers.ExecuteMemory.Slots[i].Reset();
                }

                // 2. Kill Fetch and Decode (Younger stages)
                buffers.FetchDecode.Flush();
                buffers.DecodeExecute.Flush();
                
                // IMPORTANT: Reset stalls to allow fetch from new PC immediately
                buffers.FetchDecode.IsStalled = false;
                buffers.DecodeExecute.IsStalled = false;
                
                return true;
            }

            return false;
        }
    }
}
