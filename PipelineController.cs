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
        MemWb.Rd = ExMem.Rd;
        MemWb.RegWrite = ExMem.RegWrite;
        MemWb.PC = ExMem.PC;
        MemWb.RawInstruction = ExMem.RawInstruction;
        MemWb.DecodedInst = ExMem.DecodedInst;

        ulong res = ExMem.AluResult;
        
        if (_state.Memory != null) {
            if (ExMem.MemRead) {
                if (ExMem.DecodedInst is LdInstruction)
                    res = _state.Memory.ReadDoubleWord((uint)ExMem.AluResult);
                else 
                    res = _state.Memory.ReadWord((uint)ExMem.AluResult);
            }
            else if (ExMem.MemWrite) {
                if (ExMem.DecodedInst is SdInstruction)
                    _state.Memory.WriteDoubleWord((uint)ExMem.AluResult, ExMem.WriteData);
                else
                    _state.Memory.WriteWord((uint)ExMem.AluResult, (uint)ExMem.WriteData);
            }
        }
        MemWb.FinalResult = res;
    }

    private void Stage_Execute() {
        var inst = IdEx.DecodedInst;

        ExMem.PC = IdEx.PC;
        ExMem.RawInstruction = IdEx.RawInstruction;
        ExMem.DecodedInst = inst;
        ExMem.Rd = 0;
        ExMem.RegWrite = false;
        ExMem.MemRead = false;
        ExMem.MemWrite = false;
        ExMem.AluResult = 0;
        ExMem.WriteData = 0;

        if (inst == null) return; 

        ExMem.Rd = IdEx.Rd;
        ExMem.RegWrite = IdEx.RegWrite;
        ExMem.MemRead = IdEx.MemRead;
        ExMem.MemWrite = IdEx.MemWrite;

        long v1 = 0;
        long v2 = 0;

        // SPECIAL HANDLING FOR ECALL
        if (inst is EcallInstruction) {
            // Implicitly read A7 (x17) and A0 (x10)
            v1 = (long)_state.Registers.Read(17);
            v2 = (long)_state.Registers.Read(10);
        } else {
            // Standard Read
            v1 = (long)_state.Registers.Read(inst.Rs1);
            v2 = (long)_state.Registers.Read(inst.Rs2);
        }

        // Forwarding
        if (MemWb.RegWrite && MemWb.Rd != 0) {
            // Check Explicit Dependencies
            if (MemWb.Rd == inst.Rs1) v1 = (long)MemWb.FinalResult;
            if (MemWb.Rd == inst.Rs2) v2 = (long)MemWb.FinalResult;
            
            // Check Implicit Dependencies (ECALL)
            if (inst is EcallInstruction) {
                if (MemWb.Rd == 17) v1 = (long)MemWb.FinalResult;
                if (MemWb.Rd == 10) v2 = (long)MemWb.FinalResult;
            }
        }

        ExMem.WriteData = (ulong)v2;

        long res = 0;
        long nPC = (long)IdEx.PC + 4;

        if (inst is AddiInstruction i) res = v1 + i.Imm;
        else if (inst is AluInstruction a) {
            switch (a.Mnemonic) {
                case "ADD": res = v1 + v2; break;
                case "MUL": res = v1 * v2; break;
                case "SUB": res = v1 - v2; break;
                case "SLT": res = v1 < v2 ? 1 : 0; break;
            }
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
            if (nPC < 0 || nPC >= 4096) nPC = 0; 
            _state.ProgramCounter = (ulong)nPC;
            IfId.IsValid = false;    
            IdEx.DecodedInst = null;
        }
    }

    private void Stage_Decode() {
        if (!IfId.IsValid) { 
            IdEx.DecodedInst = null; 
            IdEx.Rd = 0;
            IdEx.RegWrite = false;
            IdEx.MemRead = false;
            IdEx.MemWrite = false;
            return; 
        }

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
