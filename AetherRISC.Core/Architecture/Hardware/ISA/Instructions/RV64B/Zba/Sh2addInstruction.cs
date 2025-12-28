using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
namespace AetherRISC.Core.Architecture.Hardware.ISA.Extensions.B.Zba;

[RiscvInstruction("SH2ADD", InstructionSet.Zba, RiscvEncodingType.R, 0x33, Funct3 = 4, Funct7 = 0x10,
    Name = "Shift Left by 2 and Add", 
    Description = "Shifts rs1 left by 2 and adds it to rs2. Useful for word array indexing.", 
    Usage = "sh2add rd, rs1, rs2")]
public class Sh2addInstruction : RTypeInstruction
{
    public Sh2addInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }

    public override void Execute(MachineState s, InstructionData d) =>
        s.Registers.Write(d.Rd, (s.Registers.Read(d.Rs1) << 2) + s.Registers.Read(d.Rs2));
}
