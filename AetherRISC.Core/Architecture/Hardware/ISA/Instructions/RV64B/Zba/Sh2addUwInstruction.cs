using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
namespace AetherRISC.Core.Architecture.Hardware.ISA.Extensions.B.Zba;

[RiscvInstruction("SH2ADD.UW", InstructionSet.Zba, RiscvEncodingType.R, 0x3B, Funct3 = 4, Funct7 = 0x10,
    Name = "Shift Left by 2 and Add Unsigned Word", 
    Description = "Zero-extends rs1 to 32 bits, shifts it left by 2, and adds it to rs2.", 
    Usage = "sh2add.uw rd, rs1, rs2")]
public class Sh2addUwInstruction : RTypeInstruction
{
    public Sh2addUwInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }

    public override void Execute(MachineState s, InstructionData d)
    {
        ulong zextRs1 = s.Registers.Read(d.Rs1) & 0xFFFFFFFFul;
        s.Registers.Write(d.Rd, (zextRs1 << 2) + s.Registers.Read(d.Rs2));
    }
}

