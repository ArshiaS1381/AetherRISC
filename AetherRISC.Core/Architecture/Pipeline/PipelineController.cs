using AetherRISC.Core.Architecture;
using AetherRISC.Core.Architecture.Registers;
using AetherRISC.Core.Architecture.ISA.Base;
using AetherRISC.Core.Architecture.ISA.Decoding;
using AetherRISC.Core.Abstractions.Interfaces;

namespace AetherRISC.Core.Architecture.Pipeline;

public class PipelineController
{
    private readonly MachineState _state;
    private readonly InstructionDecoder _decoder = new InstructionDecoder();

    public IF_ID_Latch IfId { get; } = new IF_ID_Latch();
    public ID_EX_Latch IdEx { get; } = new ID_EX_Latch();
    public EX_MEM_Latch ExMem { get; } = new EX_MEM_Latch();
    public MEM_WB_Latch MemWb { get; } = new MEM_WB_Latch();

    public PipelineController(MachineState state) { _state = state; }

    public void Cycle() {
        Stage_Writeback();
        Stage_Memory();
        Stage_Execute();
        Stage_Decode();
        Stage_Fetch();
    }

    private void Stage_Writeback() { } 

    private void Stage_Memory() {
        // Latch propagation
        MemWb.Rd = ExMem.Rd;
        MemWb.RegWrite = ExMem.RegWrite;
        MemWb.PC = ExMem.PC;
        MemWb.RawInstruction = ExMem.RawInstruction;
        MemWb.DecodedInst = ExMem.DecodedInst;
        
        // Pass result through. 
        // Since Execute() updates registers immediately, and we capture that update in AluResult,
        // this carries the "Real" value (Load Data or ALU Calc) to the end.
        MemWb.FinalResult = ExMem.AluResult;
    }

    private void Stage_Execute() {
        if (IdEx.DecodedInst == null) {
            ExMem.RegWrite = false;
            ExMem.DecodedInst = null;
            return;
        }

        // --- SIMPLIFIED PIPELINE ---
        // Forwarding Logic REMOVED.
        // In this architecture, Instruction.Execute() writes to the Register File immediately.
        // Therefore, any subsequent instruction (even in the next cycle) will read the 
        // correct, updated value from the Register File automatically.
        // The previous forwarding logic was incorrectly overwriting valid register data 
        // with stale or empty latch data (the "Ghost 0" bug).

        var data = new InstructionData 
        {
            Rd = IdEx.Rd,
            Rs1 = IdEx.DecodedInst.Rs1,
            Rs2 = IdEx.DecodedInst.Rs2,
            Immediate = (ulong)(long)IdEx.DecodedInst.Imm,
            PC = IdEx.PC 
        };

        // Execute Operation (Writes to Registers immediately)
        IdEx.DecodedInst.Execute(_state, data);

        // Update Pipeline Latches
        ExMem.PC = IdEx.PC;
        ExMem.RawInstruction = IdEx.RawInstruction;
        ExMem.DecodedInst = IdEx.DecodedInst;
        ExMem.Rd = IdEx.Rd;
        ExMem.RegWrite = IdEx.RegWrite;
        ExMem.MemRead = IdEx.MemRead;
        ExMem.MemWrite = IdEx.MemWrite;
        
        // Capture the Result that was just written to the registers
        // This ensures the pipeline latches carry the correct state for debug/visualization
        if (IdEx.RegWrite && IdEx.Rd != 0) {
            ExMem.AluResult = _state.Registers.Read(IdEx.Rd);
        }
    }

    private void Stage_Decode() {
        if (!IfId.IsValid) { 
            IdEx.DecodedInst = null; IdEx.Rd = 0; IdEx.RegWrite = false; IdEx.MemRead = false; IdEx.MemWrite = false; return; 
        }

        var inst = _decoder.Decode(IfId.Instruction);
        IdEx.DecodedInst = inst;
        IdEx.RawInstruction = IfId.Instruction;
        IdEx.PC = IfId.PC;
        IdEx.Rd = inst.Rd;
        // Ensure RegWrite is False for non-writing ops to prevent latch pollution
        IdEx.RegWrite = !inst.IsStore && !inst.IsBranch && inst.Mnemonic != "BUBBLE" && inst.Mnemonic != "NOP" && inst.Mnemonic != "ECALL" && inst.Mnemonic != "FENCE" && inst.Mnemonic != "EBREAK";
        IdEx.MemRead = inst.IsLoad;
        IdEx.MemWrite = inst.IsStore;
    }

    private void Stage_Fetch() {
        uint raw = _state.Memory?.ReadWord((uint)_state.ProgramCounter) ?? 0;
        IfId.Instruction = raw;
        IfId.PC = _state.ProgramCounter;
        IfId.IsValid = true;
        _state.ProgramCounter += 4;
    }
}
