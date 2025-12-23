namespace AetherRISC.Core.Architecture.Pipeline;

public class PipelineLatch
{
    public uint InstructionWord { get; set; }
    public uint ProgramCounter { get; set; }
    public bool IsBubble { get; set; }
}
