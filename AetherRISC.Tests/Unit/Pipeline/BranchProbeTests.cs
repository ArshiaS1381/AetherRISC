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
        Init64(); // Pipeline fixture defaults 64-bit
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
        Cycle();
        
        _output.WriteLine($"[Cycle 3 End] Decode Valid? {DecodeExecuteSlot.Valid}");
        
        // Cycle 4: Mem BEQ, Ex ADDI (if survived)
        Cycle();
        _output.WriteLine($"[Cycle 4 End] Ex Buffer Result: {ExecuteMemorySlot.AluResult}");

        // x3 = 99 means ADDI passed through
        Assert.Equal(99ul, ExecuteMemorySlot.AluResult); 
    }
}
