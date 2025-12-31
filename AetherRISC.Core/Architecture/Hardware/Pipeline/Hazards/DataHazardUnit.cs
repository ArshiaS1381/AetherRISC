using AetherRISC.Core.Architecture.Hardware.Pipeline;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture;

namespace AetherRISC.Core.Architecture.Hardware.Pipeline.Hazards
{
    public class DataHazardUnit : IHazardUnit
    {
        public MachineState? StateContext { get; set; }
        public ArchitectureSettings? Settings { get; set; }

        public bool DetectAndHandle(PipelineBuffers buffers)
        {
            return Resolve(buffers);
        }

        public bool Resolve(PipelineBuffers buffers)
        {
            if (Settings == null) return false;

            var idExSlots = buffers.DecodeExecute.Slots;
            var exMemSlots = buffers.ExecuteMemory.Slots;
            int width = buffers.Width;

            bool stall = false;

            for (int i = 0; i < width; i++)
            {
                var currentOp = idExSlots[i];
                if (!currentOp.Valid || currentOp.IsBubble) continue;
                if (stall) break;

                int rs1 = currentOp.Rs1;
                int rs2 = currentOp.Rs2;

                // 1. Intra-stage RAW (Cascaded execution disabled)
                if (!Settings.AllowCascadedExecution)
                {
                    for (int j = 0; j < i; j++)
                    {
                        var older = idExSlots[j];
                        if (older.Valid && older.RegWrite && older.Rd != 0)
                        {
                            if (older.Rd == rs1 || older.Rd == rs2)
                            {
                                stall = true;
                                break;
                            }
                        }
                    }
                }
                if (stall) break;

                // 2. Load-Use Hazard
                // Check against instructions currently in Execute (outputs going to Memory)
                for(int j = 0; j < width; j++)
                {
                    var exOp = exMemSlots[j];
                    if(exOp.Valid && !exOp.IsBubble && exOp.MemRead && exOp.Rd != 0)
                    {
                        // If load target matches our sources
                        if(exOp.Rd == rs1 || exOp.Rd == rs2) 
                        { 
                            stall = true;
                            break;
                        }
                    }
                }
                if (stall) break;
            }

            if (stall)
            {
                // Freeze Fetch and Decode
                buffers.FetchDecode.IsStalled = true;
                buffers.DecodeExecute.IsStalled = true;
                return true;
            }

            return false;
        }
    }
}
