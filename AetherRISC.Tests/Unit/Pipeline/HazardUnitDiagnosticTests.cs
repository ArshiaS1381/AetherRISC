using Xunit;
using Xunit.Abstractions;
using AetherRISC.Core.Architecture.Hardware.Pipeline;
using AetherRISC.Core.Architecture.Hardware.Pipeline.Hazards;
using AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64I; // For AddInstruction
using AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64I; // For LwInstruction (via IType)

namespace AetherRISC.Tests.Unit.Pipeline;

public class HazardUnitDiagnosticTests
{
    private readonly ITestOutputHelper _output;
    public HazardUnitDiagnosticTests(ITestOutputHelper output) => _output = output;

    [Fact]
    public void StructuralUnit_Must_Stall_When_Load_Is_In_Memory_Stage()
    {
        // SETUP
        var buffers = new PipelineBuffers();
        var structUnit = new StructuralHazardUnit();

        // 1. Put a LOAD instruction in the Memory Stage (ExecuteMemory buffer)
        // Represents: LW x1, 0(x10) moving from Ex to Mem
        buffers.ExecuteMemory.IsEmpty = false;
        buffers.ExecuteMemory.MemRead = true; // IT IS A LOAD
        buffers.ExecuteMemory.Rd = 1;         // Destination is x1

        // 2. Put a CONSUMER in the Decode Stage (FetchDecode buffer)
        // Represents: ADD x2, x1, x0 (Depends on x1)
        buffers.FetchDecode.IsValid = true;
        buffers.FetchDecode.IsEmpty = false;
        // Encode: ADD x2, x1, x0 -> opcode 0x33, rd=2, rs1=1, rs2=0
        // Bin: 0000000 00000 00001 000 00010 0110011
        buffers.FetchDecode.Instruction = 0x00008133; 

        // ACT
        bool stallSignal = structUnit.DetectAndHandle(buffers);

        // ASSERT
        if (!stallSignal)
        {
            _output.WriteLine("FAILURE: StructuralHazardUnit did NOT stall.");
            _output.WriteLine("Reason: It likely only checks the Execute stage, ignoring loads in the Memory stage.");
        }
        Assert.True(stallSignal, "Pipeline MUST stall because x1 is being loaded from memory and is not ready.");
    }

    [Fact]
    public void DataUnit_Must_NOT_Forward_Address_From_Load()
    {
        // SETUP
        var buffers = new PipelineBuffers();
        var dataUnit = new DataHazardUnit();

        // 1. Put a LOAD instruction in the Execute Stage
        // Represents: LW x1, 0(x10)
        // The AluResult contains the ADDRESS (e.g. 0x100), NOT the data (e.g. 5).
        buffers.ExecuteMemory.IsEmpty = false;
        buffers.ExecuteMemory.RegWrite = true;
        buffers.ExecuteMemory.MemRead = true; // IT IS A LOAD
        buffers.ExecuteMemory.Rd = 1;
        buffers.ExecuteMemory.AluResult = 0xDEADBEEF; // The Address

        // 2. Put a CONSUMER in the Decode Stage
        // Represents: ADD x2, x1, x0
        buffers.DecodeExecute.IsEmpty = false;
        // Mock a decoded instruction
        buffers.DecodeExecute.DecodedInst = new AddInstruction(2, 1, 0); 

        // ACT
        dataUnit.Resolve(buffers);

        // ASSERT
        // We expect ForwardedRs1 to be NULL. We cannot forward from a Load in Execute!
        // If it forwards 0xDEADBEEF, the bug is present.
        
        if (buffers.DecodeExecute.ForwardedRs1 == 0xDEADBEEF)
        {
            _output.WriteLine("FAILURE: DataHazardUnit forwarded the Memory Address as Data!");
        }

        Assert.Null(buffers.DecodeExecute.ForwardedRs1);
    }
}
