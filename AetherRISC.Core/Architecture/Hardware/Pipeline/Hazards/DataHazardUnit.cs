using AetherRISC.Core.Architecture.Hardware.Pipeline;

namespace AetherRISC.Core.Architecture.Hardware.Pipeline.Hazards;

public class DataHazardUnit
{
    public void Resolve(PipelineBuffers buffers)
    {
        var idEx = buffers.DecodeExecute;
        var exMem = buffers.ExecuteMemory;
        var memWb = buffers.MemoryWriteback;

        if (idEx.IsEmpty || idEx.DecodedInst == null) return;

        var rs1 = idEx.DecodedInst.Rs1;
        var rs2 = idEx.DecodedInst.Rs2;

        idEx.ForwardedRs1 = null;
        idEx.ForwardedRs2 = null;

        if (rs1 != 0)
        {
            // Priority 1: Forward from Execute (Fast Path)
            // FIX: Added !exMem.MemRead check. Never forward Address as Data.
            if (!exMem.IsEmpty && exMem.RegWrite && exMem.Rd == rs1 && !exMem.MemRead)
                idEx.ForwardedRs1 = exMem.AluResult;
            // Priority 2: Forward from Writeback (Data is now loaded)
            else if (!memWb.IsEmpty && memWb.RegWrite && memWb.Rd == rs1)
                idEx.ForwardedRs1 = memWb.FinalResult;
        }

        if (rs2 != 0)
        {
            // FIX: Added !exMem.MemRead check.
            if (!exMem.IsEmpty && exMem.RegWrite && exMem.Rd == rs2 && !exMem.MemRead)
                idEx.ForwardedRs2 = exMem.AluResult;
            else if (!memWb.IsEmpty && memWb.RegWrite && memWb.Rd == rs2)
                idEx.ForwardedRs2 = memWb.FinalResult;
        }
    }
}
