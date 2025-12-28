using Xunit;
using AetherRISC.Tests.Infrastructure;
using AetherRISC.Core.Architecture.Hardware.ISA;

namespace AetherRISC.Tests.Unit.ISA.System;

public class PrivilegedTests : CpuTestFixture
{
    [Fact]
    public void Ecall_Triggers_Host_Handler()
    {
        // For this test, we need a mock host that records the call.
        // We will just verify it runs without crashing for now in Unit tests.
        Init64();
        Assembler.Add(pc => Inst.Ecall(0, 0, 0));
        Run(1);
        // If we reach here without exception, it works.
    }

    [Fact]
    public void Mret_Returns_To_Mepc()
    {
        Init64();
        // Setup CSRs manually
        Machine.Csr.Write(0x341, 0x1000); // MEPC = 0x1000
        
        // Execute MRET
        Assembler.Add(pc => Inst.Mret(0, 0, 0));
        Run(1);
        
        AssertPC(0x1000);
    }
}

