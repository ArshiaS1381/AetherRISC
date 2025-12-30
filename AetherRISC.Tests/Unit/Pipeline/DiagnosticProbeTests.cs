using Xunit;
using Xunit.Abstractions;
using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Tests.Infrastructure;

namespace AetherRISC.Tests.Unit.Pipeline;

public class DiagnosticProbeTests : PipelineTestFixture
{
    private readonly ITestOutputHelper _output;
    public DiagnosticProbeTests(ITestOutputHelper output) => _output = output;

    [Fact]
    public void Probe_Triple_Stress_CycleByCycle()
    {
        Init64();
        
        // 1. ADDI x1, x0, 10
        Assembler.Add(pc => Inst.Addi(1, 0, 10));
        // 2. ADDI x1, x1, 5
        Assembler.Add(pc => Inst.Addi(1, 1, 5));
        // 3. ADDI x1, x1, 1
        Assembler.Add(pc => Inst.Addi(1, 1, 1));
        
        LoadProgram();

        Cycle(1); // Fetch
        Cycle(1); // Dec
        Cycle(1); // Ex 1
        
        Cycle(1); // Ex 2
        var exOp = ExecuteMemorySlot;
        _output.WriteLine($"[Cycle 4] Exec #2 Result: {exOp.AluResult}");
        Assert.Equal(15ul, exOp.AluResult);

        Cycle(1); // Ex 3
        exOp = ExecuteMemorySlot;
        var idOp = DecodeExecuteSlot;
        
        _output.WriteLine($"[Cycle 5 Input] Rs1: {idOp.DecodedInst?.Rs1}, ForwardedRs1: {idOp.ForwardedRs1}");
        _output.WriteLine($"[Cycle 5 Result] AluResult: {exOp.AluResult}");

        Assert.Equal(16ul, exOp.AluResult);
    }
}
