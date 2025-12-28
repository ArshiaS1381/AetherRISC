using Xunit;
using Xunit.Abstractions;
using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Tests.Infrastructure;

namespace AetherRISC.Tests.Unit.Pipeline;

public class SortHazardProbe : PipelineTestFixture
{
    private readonly ITestOutputHelper _output;
    public SortHazardProbe(ITestOutputHelper output) => _output = output;

    [Fact]
    public void Prove_Pointer_Vs_Value_Hazard()
    {
        // HYPOTHESIS: The pipeline is forwarding the Address (from ALU) 
        // instead of the Value (from Memory) to the Branch unit.
        
        InitPipeline();

        // Setup Memory
        // Addr 0x100: Value 0xFFFFFFFF (Max UInt)
        // Addr 0x104: Value 0x00000001 (Min Positive)
        Machine.Memory.WriteWord(0x100, 0xFFFFFFFF);
        Machine.Memory.WriteWord(0x104, 0x00000001);
        
        // Setup Pointers
        Machine.Registers.Write(1, 0x100);
        Machine.Registers.Write(2, 0x104);

        // Code Sequence
        // 1. LW x10, 0(x1)  -> Loads 0xFFFFFFFF
        // 2. LW x11, 0(x2)  -> Loads 0x00000001
        // 3. BLTU x10, x11, FAIL_LABEL
        
        // LOGIC:
        // If comparing VALUES:   0xFFFFFFFF < 0x00000001 is FALSE. (Branch NOT Taken)
        // If comparing POINTERS: 0x00000100 < 0x00000104 is TRUE.  (Branch TAKEN)
        
        Assembler.Add(pc => Inst.Lw(10, 1, 0));
        Assembler.Add(pc => Inst.Lw(11, 2, 0));
        
        // Branch if x10 < x11 (unsigned)
        Assembler.Add(pc => Inst.Bltu(10, 11, 8)); 
        
        // Success Path (Fallthrough)
        Assembler.Add(pc => Inst.Addi(20, 0, 1)); // x20 = 1 (SUCCESS)
        Assembler.Add(pc => Inst.Ebreak(0, 0, 1));

        // Fail Path (Target)
        Assembler.Add(pc => Inst.Addi(20, 0, 0)); // x20 = 0 (FAIL)
        Assembler.Add(pc => Inst.Ebreak(0, 0, 1));
        
        LoadProgram();

        _output.WriteLine("Running Probe...");
        Cycle(10);
        
        ulong result = Machine.Registers.Read(20);
        
        if (result == 0)
        {
            _output.WriteLine("FAILURE DETECTED: Branch was taken!");
            _output.WriteLine("The CPU compared 0x100 < 0x104 (Addresses) instead of 0xFF.. < 0x1 (Values).");
            _output.WriteLine("This confirms the DataHazardUnit is forwarding ALU addresses for Loads.");
        }
        else
        {
            _output.WriteLine("SUCCESS: Branch not taken. CPU compared values correctly.");
        }

        Assert.True(result == 1, "The pipeline forwarded a pointer address instead of loaded data.");
    }
}
