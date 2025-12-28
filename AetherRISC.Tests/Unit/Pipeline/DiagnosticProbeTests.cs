using Xunit;
using Xunit.Abstractions;
using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Hardware.ISA.Encoding;
using AetherRISC.Tests.Infrastructure;

namespace AetherRISC.Tests.Unit.Pipeline;

public class DiagnosticProbeTests : PipelineTestFixture
{
    private readonly ITestOutputHelper _output;
    public DiagnosticProbeTests(ITestOutputHelper output) => _output = output;

    [Fact]
    public void Probe_Triple_Stress_CycleByCycle()
    {
        InitPipeline();
        
        // 1. ADDI x1, x0, 10
        Assembler.Add(pc => Inst.Addi(1, 0, 10));
        // 2. ADDI x1, x1, 5
        Assembler.Add(pc => Inst.Addi(1, 1, 5));
        // 3. ADDI x1, x1, 1
        Assembler.Add(pc => Inst.Addi(1, 1, 1));
        
        LoadProgram();

        // Cycle 1: Fetch #1
        Cycle();
        // Cycle 2: Dec #1
        Cycle();
        // Cycle 3: Ex #1 (Result 10)
        Cycle();
        
        // Cycle 4: Ex #2 (Should be 15)
        Cycle();
        var exBuffer = Pipeline.Buffers.ExecuteMemory;
        _output.WriteLine($"[Cycle 4] Exec #2 Result: {exBuffer.AluResult}");
        Assert.Equal(15ul, exBuffer.AluResult);

        // Cycle 5: Ex #3 (Should be 16)
        // This is where the Runner claims to get 19.
        Cycle();
        exBuffer = Pipeline.Buffers.ExecuteMemory;
        
        var idEx = Pipeline.Buffers.DecodeExecute; // Check what came into Execute
        _output.WriteLine($"[Cycle 5 Input] Rs1: {idEx.DecodedInst?.Rs1}, ForwardedRs1: {idEx.ForwardedRs1}");
        _output.WriteLine($"[Cycle 5 Result] AluResult: {exBuffer.AluResult}");

        // If this asserts 16, then the Runner loop itself is broken.
        // If this reports 19, we have caught the bug.
        Assert.Equal(16ul, exBuffer.AluResult);
    }
}
