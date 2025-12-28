using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
namespace AetherRISC.Core.Architecture.Hardware.ISA.Extensions.B.Zba;

[RiscvInstruction("ADD.UW", InstructionSet.Zba, RiscvEncodingType.R, 0x3B, Funct3 = 0, Funct7 = 0x04,
    Name = "Add Unsigned Word", 
    Description = "Adds rs1 to the zero-extended lower 32 bits of rs2. Useful for 32-bit pointer arithmetic on 64-bit systems.", 
    Usage = "add.uw rd, rs1, rs2")]
public class AddUwInstruction : RTypeInstruction
{
    public AddUwInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }

    public override void Execute(MachineState s, InstructionData d)
    {
        ulong rs1Val = s.Registers.Read(d.Rs1);
        ulong rs2Zext = s.Registers.Read(d.Rs2) & 0xFFFFFFFFul;
        s.Registers.Write(d.Rd, rs1Val + rs2Zext);
    }
}
