using System.Numerics;
using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
namespace AetherRISC.Core.Architecture.Hardware.ISA.Extensions.B.Zbb;

[RiscvInstruction("CTZW", InstructionSet.Zbb, RiscvEncodingType.ZbbUnary, 0x1B, Funct3 = 1, Funct7 = 0x30, Rs2Sel = 1)]
public class CtzwInstruction : RTypeInstruction
{
    public CtzwInstruction(int rd, int rs1, int rs2 = 0) : base(rd, rs1, rs2) { }

    public override void Execute(MachineState s, InstructionData d)
    {
        ulong val = s.Registers.Read(d.Rs1);
        uint v = (uint)val; ulong res = (uint)BitOperations.TrailingZeroCount(v);
        s.Registers.Write(d.Rd, res);
    }
}
