using AetherRISC.Core.Architecture.Hardware.Pipeline;
using AetherRISC.Core.Architecture;

namespace AetherRISC.Core.Architecture.Hardware.Pipeline.Hazards
{
    public class StructuralHazardUnit : IHazardUnit
    {
        private readonly ArchitectureSettings _settings;

        public StructuralHazardUnit(ArchitectureSettings settings)
        {
            _settings = settings;
        }

        public bool DetectAndHandle(PipelineBuffers buffers)
        {
            if (buffers.DecodeExecute.IsEmpty || buffers.DecodeExecute.IsStalled) return false;

            var slots = buffers.DecodeExecute.Slots;
            int width = buffers.Width;
            
            // Available Execution Ports per cycle
            int availInt = _settings.MaxIntALUs == 0 ? 999 : _settings.MaxIntALUs;
            int availFloat = _settings.MaxFloatALUs == 0 ? 999 : _settings.MaxFloatALUs;
            int availMem = _settings.MaxMemoryUnits == 0 ? 999 : _settings.MaxMemoryUnits;
            int availBranch = _settings.MaxBranchUnits == 0 ? 999 : _settings.MaxBranchUnits;
            int availVec = _settings.MaxVectorUnits == 0 ? 999 : _settings.MaxVectorUnits;

            bool stall = false;
            ulong stallPC = 0;

            for (int i = 0; i < width; i++)
            {
                var op = slots[i];
                if (!op.Valid || op.IsBubble) continue;

                // Determine required Unit
                bool needsMem = op.MemRead || op.MemWrite;
                bool needsBranch = (op.DecodedInst?.IsBranch ?? false) || (op.DecodedInst?.IsJump ?? false);
                bool needsFloat = op.IsFloatRegWrite; // Approximation
                bool needsVec = op.RawInstruction.ToString().Contains("V"); // Rudimentary check, ideally add IsVector to IInstruction
                
                bool dispatched = false;

                if (needsMem) 
                {
                    if (availMem > 0) { availMem--; dispatched = true; }
                }
                else if (needsBranch)
                {
                    if (availBranch > 0) { availBranch--; dispatched = true; }
                }
                else if (needsFloat)
                {
                    if (availFloat > 0) { availFloat--; dispatched = true; }
                }
                else if (needsVec)
                {
                    if (availVec > 0) { availVec--; dispatched = true; }
                }
                else
                {
                    // Default Integer
                    if (availInt > 0) { availInt--; dispatched = true; }
                }

                if (!dispatched)
                {
                    // Resource conflict. Stall this instruction and all subsequent ones in bundle.
                    stall = true;
                    stallPC = op.PC;
                    // Invalidate this op so it doesn't execute
                    op.Reset(); 
                    // Subsequent ops in loop will also be reset or skipped
                }
                else if (stall)
                {
                    // Already stalled a previous op in bundle
                    op.Reset();
                }
            }

            if (stall)
            {
                // Push back pressure
                buffers.FetchDecode.IsStalled = true;
                // Note: The controller/FetchStage handles PC rewind via detecting the stall
                // But specifically, we must ensure the PC is set back to the first stalled instruction
                // This logic often requires access to MachineState.ProgramCounter
                return true; 
            }
            
            return false;
        }
    }
}
