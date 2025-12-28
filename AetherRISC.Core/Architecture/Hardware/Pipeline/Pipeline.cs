using System;
using AetherRISC.Core.Abstractions.Interfaces;
using AetherRISC.Core.Architecture.Hardware.ISA;

namespace AetherRISC.Core.Architecture.Hardware.Pipeline
{
    public class PipelineBuffers
    {
        public FetchDecodeBuffer FetchDecode { get; set; } = new();
        public DecodeExecuteBuffer DecodeExecute { get; set; } = new();
        public ExecuteMemoryBuffer ExecuteMemory { get; set; } = new();
        public MemoryWritebackBuffer MemoryWriteback { get; set; } = new();

        public void Flush()
        {
            FetchDecode.Flush();
            DecodeExecute.Flush();
            ExecuteMemory.Flush();
            MemoryWriteback.Flush();
        }
    }

    public class FetchDecodeBuffer
    {
        public uint Instruction { get; set; }
        public ulong PC { get; set; }
        public bool IsStalled { get; set; }
        public bool IsValid { get; set; } = false;
        public bool IsEmpty { get; set; } = true;

        public void Flush()
        {
            Instruction = 0;
            PC = 0;
            IsStalled = false;
            IsValid = false;
            IsEmpty = true;
        }
    }

    public class DecodeExecuteBuffer
    {
        public IInstruction? DecodedInst { get; set; }
        public uint RawInstruction { get; set; }
        public ulong PC { get; set; }
        
        public int Rd { get; set; }
        public int Immediate { get; set; }
        
        public bool RegWrite { get; set; }
        public bool MemRead { get; set; }
        public bool MemWrite { get; set; }
        
        public ulong? ForwardedRs1 { get; set; }
        public ulong? ForwardedRs2 { get; set; }

        public bool IsEmpty { get; set; } = true;

        public void Flush()
        {
            DecodedInst = null;
            RawInstruction = 0;
            PC = 0;
            Rd = 0;
            Immediate = 0;
            RegWrite = false;
            MemRead = false;
            MemWrite = false;
            ForwardedRs1 = null;
            ForwardedRs2 = null;
            IsEmpty = true;
        }
    }

    public class ExecuteMemoryBuffer
    {
        public IInstruction? DecodedInst { get; set; }
        public uint RawInstruction { get; set; }
        public ulong PC { get; set; }
        
        public ulong AluResult { get; set; }
        public ulong StoreValue { get; set; }
        public int Rd { get; set; }
        
        public bool RegWrite { get; set; }
        public bool MemRead { get; set; }
        public bool MemWrite { get; set; }
        
        // CRITICAL FIX: This field must exist and be cleared in Flush()
        public bool BranchTaken { get; set; } 

        public bool IsEmpty { get; set; } = true;

        public void Flush()
        {
            DecodedInst = null;
            RawInstruction = 0;
            PC = 0;
            AluResult = 0;
            StoreValue = 0;
            Rd = 0;
            RegWrite = false;
            MemRead = false;
            MemWrite = false;
            BranchTaken = false; // FIX: Reset signal to prevent loops
            IsEmpty = true;
        }
    }

    public class MemoryWritebackBuffer
    {
        public IInstruction? DecodedInst { get; set; }
        public uint RawInstruction { get; set; }
        public ulong PC { get; set; }
        
        public ulong FinalResult { get; set; }
        public int Rd { get; set; }
        
        public bool RegWrite { get; set; }
        
        public bool IsEmpty { get; set; } = true;

        public void Flush()
        {
            DecodedInst = null;
            RawInstruction = 0;
            PC = 0;
            FinalResult = 0;
            Rd = 0;
            RegWrite = false;
            IsEmpty = true;
        }
    }
}
