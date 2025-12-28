using AetherRISC.Core.Architecture.Hardware.Pipeline.Controller;
using AetherRISC.Core.Architecture.Hardware.ISA.Encoding;
using AetherRISC.Tests.Infrastructure;

namespace AetherRISC.Tests.Unit.Pipeline;

public abstract class PipelineTestFixture : CpuTestFixture
{
    protected PipelineController Pipeline = null!;

    protected void InitPipeline()
    {
        Init64(); 
        Pipeline = new PipelineController(Machine);
    }

    protected void LoadProgram()
    {
        var insts = Assembler.Assemble();
        uint addr = (uint)Machine.Config.ResetVector;
        foreach (var inst in insts)
        {
            uint raw = InstructionEncoder.Encode(inst);
            Memory.WriteWord(addr, raw);
            addr += 4;
        }
        Machine.ProgramCounter = Machine.Config.ResetVector;
    }

    protected void Cycle() => Pipeline.Cycle();
    
    protected void Cycle(int count) 
    {
        for(int i=0; i<count; i++) Pipeline.Cycle();
    }
}
