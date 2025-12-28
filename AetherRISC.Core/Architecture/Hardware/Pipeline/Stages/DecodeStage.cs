using AetherRISC.Core.Abstractions.Interfaces;
using AetherRISC.Core.Architecture.Hardware.ISA.Decoding;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.Pipeline;
using AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RvSystem; // Required for Ebreak

namespace AetherRISC.Core.Architecture.Hardware.Pipeline.Stages;

public class DecodeStage
{
    private readonly MachineState _state;
    private readonly InstructionDecoder _decoder;
    private readonly IInstruction _implicitHalt;

    public DecodeStage(MachineState state)
    {
        _state = state;
        _decoder = new InstructionDecoder();
        // Create a cached "Trap/Halt" instruction to inject on errors
        _implicitHalt = new EbreakInstruction(0, 0, 1); 
    }

    public void Run(PipelineBuffers buffers)
    {
        // 1. Handle Invalid/Empty Inputs
        if (!buffers.FetchDecode.IsValid || buffers.FetchDecode.IsEmpty)
        {
            buffers.DecodeExecute.Flush();
            return;
        }

        // 2. Handle Stalls
        if (buffers.FetchDecode.IsStalled) 
        {
            buffers.DecodeExecute.Flush();
            return;
        }

        uint raw = buffers.FetchDecode.Instruction;
        ulong pc = buffers.FetchDecode.PC;

        IInstruction? inst = _decoder.Decode(raw);

        // 3. Handle Decode Failures (End of Stream / Illegal Instruction)
        // CRITICAL FIX: Do NOT halt immediately. Inject EBREAK to drain pipeline.
        if (inst == null)
        {
            inst = _implicitHalt;
        }

        // 4. Populate Output Buffer
        buffers.DecodeExecute.DecodedInst = inst;
        buffers.DecodeExecute.RawInstruction = raw;
        buffers.DecodeExecute.PC = pc;
        buffers.DecodeExecute.Rd = inst.Rd;
        buffers.DecodeExecute.Immediate = inst.Imm;
        
        buffers.DecodeExecute.MemRead = inst.IsLoad;
        buffers.DecodeExecute.MemWrite = inst.IsStore;
        // Ensure EBREAK/Branch doesn't trigger RegWrite even if Rd is aliased (unlikely but safe)
        buffers.DecodeExecute.RegWrite = (inst.Rd != 0) && !inst.IsStore && !inst.IsBranch;

        buffers.DecodeExecute.ForwardedRs1 = null;
        buffers.DecodeExecute.ForwardedRs2 = null;

        buffers.DecodeExecute.IsEmpty = false;
    }
}
