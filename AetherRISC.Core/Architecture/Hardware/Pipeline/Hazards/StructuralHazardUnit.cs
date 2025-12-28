using AetherRISC.Core.Architecture.Hardware.Pipeline;

namespace AetherRISC.Core.Architecture.Hardware.Pipeline.Hazards;

public class StructuralHazardUnit : IHazardUnit
{
    public bool DetectAndHandle(PipelineBuffers buffers)
    {
        var ifId = buffers.FetchDecode;
        
        if (!ifId.IsValid || ifId.IsEmpty) 
        {
            ifId.IsStalled = false;
            return false;
        }

        uint raw = ifId.Instruction;
        uint opcode = raw & 0x7F;
        int rs1 = (int)((raw >> 15) & 0x1F);
        int rs2 = (int)((raw >> 20) & 0x1F);

        bool isItype = opcode == 0x13 || opcode == 0x03 || opcode == 0x67 || opcode == 0x73;
        bool isLuiAuipcJal = opcode == 0x37 || opcode == 0x17 || opcode == 0x6F;
        bool usesRs1 = !isLuiAuipcJal;
        bool usesRs2 = !isItype && !isLuiAuipcJal;

        bool stall = false;

        // 1. Check Load in Execute Stage
        var idEx = buffers.DecodeExecute;
        if (!idEx.IsEmpty && idEx.MemRead && idEx.Rd != 0)
        {
            if (usesRs1 && rs1 == idEx.Rd) stall = true;
            if (usesRs2 && rs2 == idEx.Rd) stall = true;
        }

        // 2. Check Load in Memory Stage
        var exMem = buffers.ExecuteMemory;
        if (!exMem.IsEmpty && exMem.MemRead && exMem.Rd != 0)
        {
            if (usesRs1 && rs1 == exMem.Rd) stall = true;
            if (usesRs2 && rs2 == exMem.Rd) stall = true;
        }

        if (stall)
        {
            ifId.IsStalled = true;
            return true;
        }

        buffers.FetchDecode.IsStalled = false;
        return false;
    }
}
