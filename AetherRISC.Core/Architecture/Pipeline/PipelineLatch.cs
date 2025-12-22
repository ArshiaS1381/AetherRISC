namespace AetherRISC.Core.Architecture.Pipeline;

/// <summary>
/// Holds the transient state between pipeline stages.
/// </summary>
public class PipelineLatch
{
    public uint InstructionWord { get; set; }
    public uint ProgramCounter { get; set; }
    
    // Used to signal a bubble/flush
    public bool IsBubble { get; set; }
}
