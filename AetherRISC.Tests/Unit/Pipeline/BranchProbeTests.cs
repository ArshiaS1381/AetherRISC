using Xunit;
using Xunit.Abstractions;
using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Tests.Infrastructure;

namespace AetherRISC.Tests.Unit.Pipeline;

public class BranchProbeTests : PipelineTestFixture
{
    private readonly ITestOutputHelper _output;
    public BranchProbeTests(ITestOutputHelper output) => _output = output;

    [Fact]
    public void Probe_Branch_Not_Taken_Behavior()
    {
        // GOAL: Verify if the instruction immediately following a NOT-TAKEN branch executes.
        // Setup:
        // x1 = 0, x2 = 1.
        // BEQ x1, x2, +100 (0 != 1, so NOT TAKEN).
        // ADDI x3, x0, 99  (Should execute).
        
        InitPipeline();
        Machine.Registers.Write(1, 0);
        Machine.Registers.Write(2, 1);

        Assembler.Add(pc => Inst.Beq(1, 2, 100)); // Not Taken
        Assembler.Add(pc => Inst.Addi(3, 0, 99)); // Should run
        
        LoadProgram();

        // Cycle 1: Fetch BEQ
        Cycle();
        // Cycle 2: Dec BEQ, Fetch ADDI
        Cycle();
        // Cycle 3: Ex BEQ. Dec ADDI.
        // If ControlHazardUnit is buggy, it will see BEQ in EX and FLUSH Dec (ADDI).
        Cycle();
        
        var decBuffer = Pipeline.Buffers.DecodeExecute;
        _output.WriteLine($"[Cycle 3 End] Decode Buffer Empty? {decBuffer.IsEmpty}");
        
        // Cycle 4: Mem BEQ, Ex ADDI (if survived)
        Cycle();
        var exBuffer = Pipeline.Buffers.ExecuteMemory;
        _output.WriteLine($"[Cycle 4 End] Ex Buffer Result: {exBuffer.AluResult}");

        // If this is 0, the ADDI was killed.
        Assert.Equal(99ul, exBuffer.AluResult); 
    }
}
