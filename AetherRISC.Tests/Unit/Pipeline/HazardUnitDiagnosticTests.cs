using Xunit;
using Xunit.Abstractions;
using AetherRISC.Core.Architecture.Hardware.Pipeline;
using AetherRISC.Core.Architecture.Hardware.Pipeline.Hazards;
using AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64I; 

namespace AetherRISC.Tests.Unit.Pipeline;

public class HazardUnitDiagnosticTests
{
    private readonly ITestOutputHelper _output;
    public HazardUnitDiagnosticTests(ITestOutputHelper output) => _output = output;

    [Fact]
    public void StructuralUnit_Must_Stall_When_Load_Is_In_Memory_Stage()
    {
        var buffers = new PipelineBuffers(1);
        var structUnit = new StructuralHazardUnit();

        // 1. Load in Memory Stage
        buffers.ExecuteMemory.SetHasContent();
        var memOp = buffers.ExecuteMemory.Slots[0];
        memOp.Valid = true;
        memOp.MemRead = true;
        memOp.Rd = 1;

        // 2. Consumer in Decode Stage (ADD x2, x1, x0)
        buffers.FetchDecode.SetHasContent();
        var ifOp = buffers.FetchDecode.Slots[0];
        ifOp.Valid = true;
        ifOp.RawInstruction = 0x00008133; 

        bool stallSignal = structUnit.DetectAndHandle(buffers);

        Assert.True(stallSignal, "Pipeline MUST stall because x1 is being loaded from memory and is not ready.");
    }

    [Fact]
    public void DataUnit_Must_NOT_Forward_Address_From_Load()
    {
        var buffers = new PipelineBuffers(1);
        var dataUnit = new DataHazardUnit();

        // 1. Load in Execute Stage
        buffers.ExecuteMemory.SetHasContent();
        var exOp = buffers.ExecuteMemory.Slots[0];
        exOp.Valid = true;
        exOp.RegWrite = true;
        exOp.MemRead = true;
        exOp.Rd = 1;
        exOp.AluResult = 0xDEADBEEF; // Address

        // 2. Consumer in Decode
        buffers.DecodeExecute.SetHasContent();
        var decOp = buffers.DecodeExecute.Slots[0];
        decOp.Valid = true;
        decOp.DecodedInst = new AddInstruction(2, 1, 0); 

        dataUnit.Resolve(buffers);

        Assert.Null(decOp.ForwardedRs1);
    }
}
