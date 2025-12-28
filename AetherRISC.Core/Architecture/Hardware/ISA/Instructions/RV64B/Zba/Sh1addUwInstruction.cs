using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
namespace AetherRISC.Core.Architecture.Hardware.ISA.Extensions.B.Zba;

[RiscvInstruction("SH1ADD.UW", InstructionSet.Zba, RiscvEncodingType.R, 0x3B, Funct3 = 2, Funct7 = 0x10,
    Name = "Shift Left by 1 and Add Unsigned Word", 
    Description = "Zero-extends rs1 to 32 bits, shifts it left by 1, and adds it to rs2.", 
    Usage = "sh1add.uw rd, rs1, rs2")]
public class Sh1addUwInstruction : RTypeInstruction
{
    public Sh1addUwInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }

    public override void Execute(MachineState s, InstructionData d)
    {
        ulong zextRs1 = s.Registers.Read(d.Rs1) & 0xFFFFFFFFul;
        s.Registers.Write(d.Rd, (zextRs1 << 1) + s.Registers.Read(d.Rs2));
    }
}

