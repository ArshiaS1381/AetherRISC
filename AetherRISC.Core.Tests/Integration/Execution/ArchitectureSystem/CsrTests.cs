using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.ISA.Types;
using Xunit;
using AetherRISC.Core.Architecture;
using AetherRISC.Core.Architecture.Hardware.Registers;
using AetherRISC.Core.Architecture.Hardware.ISA.Families;
using AetherRISC.Core.Architecture.Hardware.ISA.Instructions;
// FIX: Add missing namespace for Zicsr instructions
using AetherRISC.Core.Architecture.Hardware.ISA.Extensions.Zicsr;

namespace AetherRISC.Core.Tests.Architecture.System;

public class CsrTests
{
    private MachineState _state = new MachineState(SystemConfig.Rv64());
    
    private InstructionData Data(int rd, int rs1, int imm) 
        => new InstructionData { Rd = rd, Rs1 = rs1, Immediate = (ulong)imm };

    [Fact]
    public void CSRRW_Should_Swap_Values()
    {
        uint mepc = CsrConstants.mepc; 
        
        _state.Registers.Write(1, 0xAA);
        _state.Csr.Write(mepc, 0x55);

        var inst = new CsrrwInstruction(2, 1, (int)mepc);
        inst.Execute(_state, Data(2, 1, (int)mepc));

        Assert.Equal((ulong)0x55, _state.Registers.Read(2));
        Assert.Equal((ulong)0xAA, _state.Csr.Read(mepc));
    }

    [Fact]
    public void CSRRS_Should_Set_Bits()
    {
        uint mstatus = CsrConstants.mstatus;
        
        _state.Csr.Write(mstatus, 0x5);
        _state.Registers.Write(1, 0x2);

        var inst = new CsrrsInstruction(2, 1, (int)mstatus);
        inst.Execute(_state, Data(2, 1, (int)mstatus));

        Assert.Equal((ulong)0x5, _state.Registers.Read(2));
        Assert.Equal((ulong)0x7, _state.Csr.Read(mstatus));
    }

    [Fact]
    public void CSRRC_Should_Clear_Bits()
    {
        uint mstatus = CsrConstants.mstatus;

        _state.Csr.Write(mstatus, 0x7);
        _state.Registers.Write(1, 0x2);

        var inst = new CsrrcInstruction(2, 1, (int)mstatus);
        inst.Execute(_state, Data(2, 1, (int)mstatus));

        Assert.Equal((ulong)0x7, _state.Registers.Read(2));
        Assert.Equal((ulong)0x5, _state.Csr.Read(mstatus));
    }
}



