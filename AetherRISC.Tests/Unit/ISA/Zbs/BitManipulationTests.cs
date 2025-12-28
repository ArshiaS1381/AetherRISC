using AetherRISC.Tests.Unit.Pipeline;
using Xunit;
using AetherRISC.Tests.Infrastructure;
using AetherRISC.Core.Architecture.Hardware.ISA;

namespace AetherRISC.Tests.Unit.ISA.Zbs;

public class BitManipulationTests : PipelineTestFixture
{
    [Fact]
    public void Bset_Forwarding_RAW_Hazard()
    {
        // Test RAW hazard where BSET output is immediately consumed by BEXT
        // x1 = 1
        // x2 = 63 (Index)
        // x3 = BSET x1, x2 -> Set bit 63. Result: 0x8000...0001
        // x4 = BEXT x3, x2 -> Extract bit 63. Result: 1
        
        InitPipeline();
        
        Assembler.Add(pc => Inst.Addi(1, 0, 1));
        Assembler.Add(pc => Inst.Addi(2, 0, 63));
        Assembler.Add(pc => Inst.Bset(3, 1, 2)); // Produces value in EX stage
        Assembler.Add(pc => Inst.Bext(4, 3, 2)); // Consumes value in EX stage (needs forwarding)
        
        LoadProgram();
        
        Cycle(10); // Run past pipeline latency
        
        // x3 should be 0x8000000000000001
        AssertReg(3, 0x8000000000000001ul);
        // x4 should be 1
        AssertReg(4, 1ul);
    }

    [Fact]
    public void Binv_Bclr_Toggles_Correctly()
    {
        Init64();
        // x1 = 0
        // BSETI x1, 5 -> 32 (0x20)
        // BINVI x1, 5 -> 0  (Toggle back to 0)
        // BCLRI x1, 0 -> 0  (Clear bit 0, which is already 0)
        
        Assembler.Add(pc => Inst.Addi(1, 0, 0));
        Assembler.Add(pc => Inst.Bseti(1, 1, 5));
        Assembler.Add(pc => Inst.Binvi(1, 1, 5));
        Assembler.Add(pc => Inst.Bclri(1, 1, 0));
        
        Run(4);
        
        AssertReg(1, 0ul);
    }
}

