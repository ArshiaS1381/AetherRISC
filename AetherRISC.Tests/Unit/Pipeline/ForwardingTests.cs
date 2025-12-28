using Xunit;
using AetherRISC.Core.Architecture.Hardware.ISA;

namespace AetherRISC.Tests.Unit.Pipeline;

public class ForwardingTests : PipelineTestFixture
{
    [Fact]
    public void RAW_Saturation_Dependency_Chain()
    {
        // A "Torture" test for the forwarding unit.
        // Every instruction depends immediately on the previous one.
        // x1 = 30
        // x4 = x1 + x2 (Forward x1 from EX)
        // x5 = x4 + x1 (Forward x4 from EX, x1 from MEM)
        
        InitPipeline();
        Machine.Registers.Write(2, 10);
        Machine.Registers.Write(3, 20);

        Assembler.Add(pc => Inst.Add(1, 2, 3)); // x1 = 10 + 20 = 30
        Assembler.Add(pc => Inst.Add(4, 1, 2)); // x4 = 30 + 10 = 40
        Assembler.Add(pc => Inst.Add(5, 4, 1)); // x5 = 40 + 30 = 70
        Assembler.Add(pc => Inst.Add(6, 5, 4)); // x6 = 70 + 40 = 110
        
        LoadProgram();
        
        // C1: Fetch ADD(x1)
        // C2: Dec ADD(x1), Fetch ADD(x4)
        // C3: Ex ADD(x1), Dec ADD(x4), Fetch ADD(x5)
        // C4: Mem ADD(x1), Ex ADD(x4), Dec ADD(x5) -> Forwarding x1 from Mem to Ex(x5)? No, x5 uses x4(Ex) and x1(Mem)
        
        Cycle(10); // Run enough to clear pipeline

        AssertReg(6, 110ul);
    }

    [Fact]
    public void Triple_Forwarding_Stress()
    {
        InitPipeline();
        
        // ADDI x1, x0, 10  -> x1=10
        // ADDI x1, x1, 5   -> x1=15 (Forward 10)
        // ADDI x1, x1, 1   -> x1=16 (Forward 15)
        
        Assembler.Add(pc => Inst.Addi(1, 0, 10));
        Assembler.Add(pc => Inst.Addi(1, 1, 5));
        Assembler.Add(pc => Inst.Addi(1, 1, 1));
        
        LoadProgram();
        
        Cycle(10);
        
        AssertReg(1, 16ul);
    }
}
