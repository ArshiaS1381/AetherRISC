using System;
using AetherRISC.Core.Architecture.Registers;
using AetherRISC.Core.Hardware.ISA.Base;
using AetherRISC.Core.Hardware.ISA.Decoding;
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

    private void Stage_Writeback() {
        if (MemWb.RegWrite && MemWb.Rd != 0)
            _state.Registers.Write(MemWb.Rd, MemWb.FinalResult);
    }

    private void Stage_Memory() {
        ulong res = ExMem.AluResult;
        if (ExMem.MemRead && _state.Memory != null)
            res = (ExMem.DecodedInst is LdInstruction) ? _state.Memory.ReadDoubleWord((uint)ExMem.AluResult) : _state.Memory.ReadWord((uint)ExMem.AluResult);
        else if (ExMem.MemWrite && _state.Memory != null) {
            if (ExMem.DecodedInst is SdInstruction) _state.Memory.WriteDoubleWord((uint)ExMem.AluResult, ExMem.WriteData);
            else _state.Memory.WriteWord((uint)ExMem.AluResult, (uint)ExMem.WriteData);
        }
        MemWb.FinalResult = res;
        MemWb.Rd = ExMem.Rd;
        MemWb.RegWrite = ExMem.RegWrite;
        MemWb.PC = ExMem.PC;
        MemWb.RawInstruction = ExMem.RawInstruction;
        MemWb.DecodedInst = ExMem.DecodedInst;
    }

    private void Stage_Execute() {
        var inst = IdEx.DecodedInst;
        ExMem.PC = IdEx.PC;
        ExMem.RawInstruction = IdEx.RawInstruction;
        ExMem.DecodedInst = IdEx.DecodedInst;
        ExMem.Rd = IdEx.Rd;
        ExMem.RegWrite = IdEx.RegWrite;
        ExMem.MemRead = IdEx.MemRead;
        ExMem.MemWrite = IdEx.MemWrite;

        if (inst == null) return;

        // --- SIGNED MATH & FORWARDING ---
        long v1 = (long)_state.Registers.Read(inst.Rs1);
        long v2 = (long)_state.Registers.Read(inst.Rs2);

        if (ExMem.RegWrite && ExMem.Rd != 0) {
            if (ExMem.Rd == inst.Rs1) v1 = (long)ExMem.AluResult;
            if (ExMem.Rd == inst.Rs2) v2 = (long)ExMem.AluResult;
        }
        if (MemWb.RegWrite && MemWb.Rd != 0) {
            if (MemWb.Rd == inst.Rs1 && !(ExMem.RegWrite && ExMem.Rd == inst.Rs1)) v1 = (long)MemWb.FinalResult;
            if (MemWb.Rd == inst.Rs2 && !(ExMem.RegWrite && ExMem.Rd == inst.Rs2)) v2 = (long)MemWb.FinalResult;
        }

        ExMem.WriteData = (ulong)v2;
        long res = 0;
        long nPC = (long)IdEx.PC + 4;

        if (inst is AddiInstruction i) res = v1 + i.Imm;
        else if (inst is AluInstruction a) {
            if (a.Mnemonic == "ADD") res = v1 + v2;
            else if (a.Mnemonic == "MUL") res = v1 * v2;
            else if (a.Mnemonic == "SUB") res = v1 - v2;
            else if (a.Mnemonic == "SLT") res = v1 < v2 ? 1 : 0;
        } 
        else if (inst is JalInstruction j) { res = (long)IdEx.PC + 4; nPC = (long)IdEx.PC + j.Imm; }
        else if (inst is JalrInstruction jr) { res = (long)IdEx.PC + 4; nPC = v1 + jr.Imm; }
        else if (inst is BneInstruction b) { if (v1 != v2) nPC = (long)IdEx.PC + b.Imm; }
        else if (inst is LdInstruction ld) res = v1 + ld.Imm;
        else if (inst is SdInstruction sd) res = v1 + sd.Imm;
        else if (inst is EcallInstruction) {
            if (v1 == 1) _state.Host?.PrintInt(v2);
            else if (v1 == 10) _state.Host?.Exit(0);
        }

        ExMem.AluResult = (ulong)res;
        if (nPC != (long)IdEx.PC + 4) {
            // Safety: Clamp PC to mapped memory (4KB) to prevent rollover
            if (nPC < 0 || nPC >= 4096) nPC = 0; 
            _state.ProgramCounter = (ulong)nPC;
            IfId.IsValid = false;
            IdEx.DecodedInst = null;
        }
    }

    private void Stage_Decode() {
        if (!IfId.IsValid) { IdEx.DecodedInst = null; return; }
        var inst = _decoder.Decode(IfId.Instruction);
        IdEx.DecodedInst = inst;
        IdEx.RawInstruction = IfId.Instruction;
        IdEx.PC = IfId.PC;
        IdEx.Rd = inst.Rd;
        IdEx.RegWrite = !inst.IsStore && !inst.IsBranch && !inst.IsJump && inst.Mnemonic != "BUBBLE" && inst.Mnemonic != "NOP";
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
