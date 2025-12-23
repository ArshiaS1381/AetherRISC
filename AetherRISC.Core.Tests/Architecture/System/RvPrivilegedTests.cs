using Xunit;
using AetherRISC.Core.Architecture;
using AetherRISC.Core.Architecture.ISA.Base;

namespace AetherRISC.Core.Tests.Architecture.System;

public class RvPrivilegedTests
{
    [Fact]
    public void MRET_Should_Jump_To_MEPC()
    {
        var state = new MachineState(SystemConfig.Rv64());
        ulong returnAddress = 0x80001000;
        
        // 1. Manually set MEPC (CSR 0x341)
        state.Csr.Write(0x341, returnAddress);
        
        // 2. Execute MRET
        var mret = new MretInstruction();
        mret.Execute(state, new InstructionData());

        // 3. Verify PC updated
        Assert.Equal(returnAddress, state.ProgramCounter);
    }
}
