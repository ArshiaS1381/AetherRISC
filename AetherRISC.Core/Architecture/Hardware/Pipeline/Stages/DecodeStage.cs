using AetherRISC.Core.Abstractions.Interfaces;
using AetherRISC.Core.Architecture.Hardware.ISA.Decoding;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.Pipeline;
using AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RvSystem;

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
        _implicitHalt = new EbreakInstruction(0, 0, 1); 
    }

    public void Run(PipelineBuffers buffers)
    {
        if (!buffers.FetchDecode.IsValid || buffers.FetchDecode.IsEmpty)
        {
            buffers.DecodeExecute.Flush();
            return;
        }

        if (buffers.FetchDecode.IsStalled) 
        {
            buffers.DecodeExecute.Flush();
            return;
        }

        uint raw = buffers.FetchDecode.Instruction;
        ulong pc = buffers.FetchDecode.PC;

        IInstruction? inst = _decoder.Decode(raw) ?? _implicitHalt;

        buffers.DecodeExecute.DecodedInst = inst;
        buffers.DecodeExecute.RawInstruction = raw;
        buffers.DecodeExecute.PC = pc;
        buffers.DecodeExecute.Rd = inst.Rd;
        buffers.DecodeExecute.Immediate = inst.Imm;
        
        // --- Pass Prediction Metadata ---
        buffers.DecodeExecute.PredictedTaken = buffers.FetchDecode.PredictedTaken;
        buffers.DecodeExecute.PredictedTarget = buffers.FetchDecode.PredictedTarget;
        // --------------------------------

        buffers.DecodeExecute.MemRead = inst.IsLoad;
        buffers.DecodeExecute.MemWrite = inst.IsStore;
        buffers.DecodeExecute.RegWrite = (inst.Rd != 0) && !inst.IsStore && !inst.IsBranch;
        buffers.DecodeExecute.ForwardedRs1 = null;
        buffers.DecodeExecute.ForwardedRs2 = null;
        buffers.DecodeExecute.IsEmpty = false;
    }
}
