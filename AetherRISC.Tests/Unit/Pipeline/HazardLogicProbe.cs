using Xunit;
using Xunit.Abstractions;
using AetherRISC.Core.Architecture.Hardware.Pipeline;
using AetherRISC.Core.Architecture.Hardware.Pipeline.Hazards;
using AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64I; 

namespace AetherRISC.Tests.Unit.Pipeline;

public class HazardLogicProbe
{
    private readonly ITestOutputHelper _output;
    public HazardLogicProbe(ITestOutputHelper output) => _output = output;

    [Fact]
    public void Structural_Unit_Ignores_Memory_Stage_Load()
    {
        var unit = new StructuralHazardUnit();
        var buffers = new PipelineBuffers();

        // 1. Setup Hazard: Load in Memory Stage
        buffers.ExecuteMemory.IsEmpty = false;
        buffers.ExecuteMemory.MemRead = true; 
        buffers.ExecuteMemory.Rd = 5;         

        // 2. Setup Consumer: Instruction in Decode reading x5
        buffers.FetchDecode.IsValid = true;
        buffers.FetchDecode.IsEmpty = false;
        // Instruction: ADD x1, x5, x0 (opcode=0x33, rs1=5)
        buffers.FetchDecode.Instruction = 0x000280B3; 

        bool stall = unit.DetectAndHandle(buffers);

        if (!stall) _output.WriteLine("FAIL: StructuralHazardUnit failed to stall for a Load in Memory Stage.");
        Assert.True(stall, "Pipeline MUST stall when consumer depends on a Load in Memory stage.");
    }

    [Fact]
    public void Data_Unit_Forwards_Address_Instead_Of_Data()
    {
        var unit = new DataHazardUnit();
        var buffers = new PipelineBuffers();

        // 1. Setup Hazard: Load in Execute Stage (AluResult = Address)
        buffers.ExecuteMemory.IsEmpty = false;
        buffers.ExecuteMemory.RegWrite = true;
        buffers.ExecuteMemory.MemRead = true; // It is a Load
        buffers.ExecuteMemory.Rd = 5;
        buffers.ExecuteMemory.AluResult = 0x8000; // The ADDRESS

        // 2. Setup Consumer
        buffers.DecodeExecute.IsEmpty = false;
        buffers.DecodeExecute.DecodedInst = new AddInstruction(1, 5, 0); 

        unit.Resolve(buffers);

        ulong? fwd = buffers.DecodeExecute.ForwardedRs1;
        
        if (fwd == 0x8000) _output.WriteLine("FAIL: DataHazardUnit forwarded the Memory Address (0x8000) as data!");
        
        Assert.Null(fwd); // Should be null (stall required)
    }
}
