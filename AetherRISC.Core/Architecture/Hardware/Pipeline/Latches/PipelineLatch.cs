namespace AetherRISC.Core.Architecture.Hardware.Pipeline.Latches;

public class PipelineLatch
{
    public uint InstructionWord { get; set; }
    public uint ProgramCounter { get; set; }
    public bool IsBubble { get; set; }
}


