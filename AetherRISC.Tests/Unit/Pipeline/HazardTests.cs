using Xunit;
using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Tests.Infrastructure;

namespace AetherRISC.Tests.Unit.Pipeline;

public class HazardTests : PipelineTestFixture
{
    [Fact]
    public void RAW_Hazard_DataForwarding_Execute_To_Execute()
    {
        Init64();
        
        Assembler.Add(pc => Inst.Addi(1, 0, 10));
        Assembler.Add(pc => Inst.Add(2, 1, 0));
        
        LoadProgram();

        Cycle(1); // Fetch ADDI
        Cycle(1); // Dec ADDI, Fetch ADD
        Cycle(1); // Ex ADDI, Dec ADD (Forwarding setup here)
        Cycle(1); // Mem ADDI, Ex ADD (Result computed using forwarded val)
        
        Assert.Equal(10ul, ExecuteMemorySlot.AluResult);
    }

    [Fact]
    public void Control_Hazard_Taken_Branch_Flushes_Pipeline()
    {
        Init64();
        
        Assembler.Add(pc => Inst.Beq(0, 0, Assembler.To("target", pc)));
        Assembler.Add(pc => Inst.Addi(1, 0, 99));
        Assembler.Add(pc => Inst.Addi(2, 0, 1), "target");
        
        LoadProgram();

        Cycle(3); // C3: Exec BEQ. Flushes.
        
        Cycle(3); // Pipeline refills from target
        
        Cycle(2);
        
        AssertReg(1, 0ul);
        AssertReg(2, 1ul);
    }
}
