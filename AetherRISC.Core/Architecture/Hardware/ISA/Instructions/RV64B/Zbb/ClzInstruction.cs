using System.Numerics;
using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
namespace AetherRISC.Core.Architecture.Hardware.ISA.Extensions.B.Zbb;

[RiscvInstruction("CLZ", InstructionSet.Zbb, RiscvEncodingType.ZbbUnary, 0x13, Funct3 = 1, Funct7 = 0x30, Rs2Sel = 0)]
public class ClzInstruction : RTypeInstruction
{
    public ClzInstruction(int rd, int rs1, int rs2 = 0) : base(rd, rs1, rs2) { }

    public override void Execute(MachineState s, InstructionData d)
    {
        ulong val = s.Registers.Read(d.Rs1);
        ulong res = (s.Config.XLEN == 32) ? (uint)BitOperations.LeadingZeroCount((uint)val) : (ulong)BitOperations.LeadingZeroCount(val);
        s.Registers.Write(d.Rd, res);
    }
}
