using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
namespace AetherRISC.Core.Architecture.Hardware.ISA.Extensions.B.Zbb;

[RiscvInstruction("ORC.B", InstructionSet.Zbb, RiscvEncodingType.ShiftImm, 0x13, Funct3 = 5, Funct6 = 0x3A,
    Name = "Bitstring OR-Combine", 
    Description = "Combines bits within each byte. If any bit in a byte is 1, the whole byte becomes 0xFF, else 0x00.", 
    Usage = "orc.b rd, rs1")]
public class OrcBInstruction : ITypeInstruction
{
    public OrcBInstruction(int rd, int rs1, int imm) : base(rd, rs1, imm) { }

    public override void Execute(MachineState s, InstructionData d)
    {
        ulong v = s.Registers.Read(d.Rs1);
        ulong res = 0;
        int bytes = s.Config.XLEN / 8;

        for (int i = 0; i < bytes; i++)
        {
            int shift = i * 8;
            if (((v >> shift) & 0xFF) != 0) res |= (0xFFul << shift);
        }
        s.Registers.Write(d.Rd, res);
    }
}
