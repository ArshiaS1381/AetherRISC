using Xunit;
using Xunit.Abstractions;
using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Tests.Infrastructure;

namespace AetherRISC.Tests.Unit.Pipeline;

public class PipelineDiagnosticTests : PipelineTestFixture
{
    private readonly ITestOutputHelper _output;
    public PipelineDiagnosticTests(ITestOutputHelper output) => _output = output;

    [Fact]
    public void Forwarding_Priority_Check()
    {
        InitPipeline();
        Machine.Registers.Write(1, 5); 

        Assembler.Add(pc => Inst.Addi(1, 0, 10));
        Assembler.Add(pc => Inst.Addi(1, 0, 20));
        Assembler.Add(pc => Inst.Addi(2, 1, 0));
        
        LoadProgram();
        
        Cycle(10); 
        
        ulong result = Machine.Registers.Read(2);
        _output.WriteLine($"Priority Check: x2 = {result} (Expected 20)");
        
        Assert.Equal(20ul, result);
    }

    [Fact]
    public void Stall_Integrity_Check()
    {
        InitPipeline();
        Machine.Memory.WriteWord(0x1000, 99);
        Machine.Registers.Write(1, 0x1000);

        Assembler.Add(pc => Inst.Lw(2, 1, 0));
        Assembler.Add(pc => Inst.Addi(3, 2, 1));
        
        LoadProgram();
        
        Cycle(10);
        
        ulong valX2 = Machine.Registers.Read(2);
        ulong valX3 = Machine.Registers.Read(3);
        
        _output.WriteLine($"Stall Check: x2={valX2} (Exp 99), x3={valX3} (Exp 100)");

        Assert.Equal(99ul, valX2);
        Assert.Equal(100ul, valX3);
    }

    [Fact]
    public void Ghost_Forwarding_Check()
    {
        InitPipeline();
        
        Assembler.Add(pc => Inst.Addi(1, 0, 10)); 
        Assembler.Add(pc => Inst.Addi(0, 0, 0)); 
        Assembler.Add(pc => Inst.Addi(2, 1, 0));  
        
        LoadProgram();
        
        Cycle(10);
        
        ulong result = Machine.Registers.Read(2);
        _output.WriteLine($"Ghost Check: x2 = {result} (Expected 10)");
        
        Assert.Equal(10ul, result);
    }
}
