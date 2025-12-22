using AetherRISC.Core.Abstractions.Interfaces;

namespace AetherRISC.Core.Architecture.Pipeline;

// 1. Fetch -> Decode Latch
public class IF_ID_Latch
{
    public ulong PC { get; set; }
    public uint Instruction { get; set; }
    public bool IsValid { get; set; } = false;
}

// 2. Decode -> Execute Latch
public class ID_EX_Latch
{
    public ulong PC { get; set; }
    public uint RawInstruction { get; set; } // Added
    public IInstruction? DecodedInst { get; set; }
    
    public int Rd { get; set; } 
    public bool RegWrite { get; set; }
    public bool MemRead { get; set; }
    public bool MemWrite { get; set; }
}

// 3. Execute -> Memory Latch
public class EX_MEM_Latch
{
    public ulong PC { get; set; } // Added
    public uint RawInstruction { get; set; } // Added
    public IInstruction? DecodedInst { get; set; } // Added

    public ulong AluResult { get; set; }
    public ulong WriteData { get; set; }
    
    public int Rd { get; set; }
    public bool RegWrite { get; set; }
    public bool MemRead { get; set; }
    public bool MemWrite { get; set; }
}

// 4. Memory -> Writeback Latch
public class MEM_WB_Latch
{
    public ulong PC { get; set; } // Added
    public uint RawInstruction { get; set; } // Added
    public IInstruction? DecodedInst { get; set; } // Added

    public ulong FinalResult { get; set; }
    public int Rd { get; set; }
    public bool RegWrite { get; set; }
}
