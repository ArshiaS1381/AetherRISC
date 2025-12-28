using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.ISA.Types;
using Xunit;
using AetherRISC.Core.Architecture;
using AetherRISC.Core.Architecture.Hardware.ISA.Instructions;

namespace AetherRISC.Core.Tests.Architecture.Granular;

public class GranularBranchTests
{
    private MachineState _state = new MachineState(SystemConfig.Rv64());
    private InstructionData Data(int rd, int rs1, int rs2, int imm) 
        => new InstructionData { Rd = rd, Rs1 = rs1, Rs2 = rs2, Immediate = (ulong)(long)imm, PC = 0x100 };

    [Fact]
    public void SLT_Handles_Negative_Numbers_Correctly()
    {
        // -1 < 1 is TRUE
        // -1 = 0xFF...FF
        // 1  = 0x00...01
        _state.Registers.Write(1, ulong.MaxValue); // -1
        _state.Registers.Write(2, 1);              // 1
        
        var inst = new SltInstruction(3, 1, 2);
        inst.Execute(_state, Data(3, 1, 2, 0));
        
        Assert.Equal((ulong)1, _state.Registers.Read(3)); // True
    }

    [Fact]
    public void SLTU_Treats_Negative_As_Large_Positive()
    {
        // -1 (MaxUInt) < 1 is FALSE in Unsigned world
        _state.Registers.Write(1, ulong.MaxValue);
        _state.Registers.Write(2, 1);
        
        var inst = new SltuInstruction(3, 1, 2);
        inst.Execute(_state, Data(3, 1, 2, 0));
        
        Assert.Equal((ulong)0, _state.Registers.Read(3)); // False
    }

    // Note: We simulate Branch logic using SLT/SLTU underlying logic usually
    // But since B* instructions calculate PC, we test SLT instructions 
    // to verify the Comparator Logic specifically.
}



