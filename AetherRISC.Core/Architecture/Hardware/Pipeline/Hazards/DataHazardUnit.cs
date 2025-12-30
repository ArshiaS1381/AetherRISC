using AetherRISC.Core.Architecture.Hardware.Pipeline;
using AetherRISC.Core.Architecture.Simulation.State;

namespace AetherRISC.Core.Architecture.Hardware.Pipeline.Hazards
{
    public class DataHazardUnit : IHazardUnit
    {
        public MachineState? StateContext { get; set; }
        public ArchitectureSettings? Settings { get; set; } // Added Settings

        public bool DetectAndHandle(PipelineBuffers buffers)
        {
            return Resolve(buffers);
        }

        public bool Resolve(PipelineBuffers buffers)
        {
            var idExSlots = buffers.DecodeExecute.Slots;
            var exMemSlots = buffers.ExecuteMemory.Slots;
            int width = buffers.Width;

            bool bundleBroken = false;
            ulong replayPC = 0;
            int breakIndex = -1;

            for (int i = 0; i < width; i++)
            {
                var currentOp = idExSlots[i];

                if (bundleBroken) 
                { 
                    currentOp.Reset(); 
                    continue; 
                }

                if (!currentOp.Valid || currentOp.IsBubble) continue;

                int rs1 = currentOp.Rs1;
                int rs2 = currentOp.Rs2;

                currentOp.ForwardedRs1 = null;
                currentOp.ForwardedRs2 = null;

                // 1. INTRA-BUNDLE CHECK
                for (int j = 0; j < i; j++)
                {
                    var olderOp = idExSlots[j];
                    if (olderOp.Valid && !olderOp.IsBubble && olderOp.RegWrite && olderOp.Rd != 0)
                    {
                        if (olderOp.Rd == rs1 || olderOp.Rd == rs2)
                        {
                            bool isHazard = true;

                            // LOGIC BRANCH: Cascaded Execution
                            if (Settings != null && Settings.AllowCascadedExecution)
                            {
                                // Optimization: Only Hazard if producer is a Load.
                                // If ALU, we will forward in ExecuteStage.
                                isHazard = olderOp.MemRead;
                            }
                            // Else: Always a hazard (Simpler superscalar logic)

                            if (isHazard)
                            {
                                bundleBroken = true;
                                replayPC = currentOp.PC;
                                breakIndex = i;
                                break;
                            }
                        }
                    }
                }

                if (bundleBroken)
                {
                    currentOp.Reset();
                    continue;
                }

                // 2. LOAD-USE CHECK (Execute Stage)
                for(int j = 0; j < width; j++)
                {
                    var exOp = exMemSlots[j];
                    if(exOp.Valid && !exOp.IsBubble && exOp.MemRead && exOp.Rd != 0)
                    {
                        if(exOp.Rd == rs1 || exOp.Rd == rs2) 
                        { 
                            bundleBroken = true;
                            replayPC = currentOp.PC;
                            breakIndex = i;
                            break;
                        }
                    }
                }

                if (bundleBroken)
                {
                    currentOp.Reset();
                    continue;
                }
            }

            if (bundleBroken)
            {
                if (StateContext != null) StateContext.ProgramCounter = replayPC;
                
                buffers.FetchDecode.Flush();
                buffers.FetchDecode.IsStalled = false;
                
                return true;
            }

            return false;
        }
    }
}
