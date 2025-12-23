using AetherRISC.Core.Architecture;
using AetherRISC.Core.Abstractions.Interfaces;

namespace AetherRISC.Core.Architecture.Pipeline;

public class IF_ID_Latch
{
    public ulong PC { get; set; }
    public uint Instruction { get; set; }
    public bool IsValid { get; set; } = false;
}

public class ID_EX_Latch
{
    public ulong PC { get; set; }
    public uint RawInstruction { get; set; }
    public IInstruction? DecodedInst { get; set; }
    public int Rd { get; set; } 
    public bool RegWrite { get; set; }
    public bool MemRead { get; set; }
    public bool MemWrite { get; set; }
}

public class EX_MEM_Latch
{
    public ulong PC { get; set; }
    public uint RawInstruction { get; set; }
    public IInstruction? DecodedInst { get; set; }
    public ulong AluResult { get; set; }
    public ulong WriteData { get; set; }
    public int Rd { get; set; }
    public bool RegWrite { get; set; }
    public bool MemRead { get; set; }
    public bool MemWrite { get; set; }
}

public class MEM_WB_Latch
{
    public ulong PC { get; set; }
    public uint RawInstruction { get; set; }
    public IInstruction? DecodedInst { get; set; }
    public ulong FinalResult { get; set; }
    public int Rd { get; set; }
    public bool RegWrite { get; set; }
}
