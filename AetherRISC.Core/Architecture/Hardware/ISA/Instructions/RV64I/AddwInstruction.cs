using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
namespace AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64I;

[RiscvInstruction("ADDW", InstructionSet.RV64I, RiscvEncodingType.R, 0x3B, Funct3 = 0, Funct7 = 0,
    Name = "Add Word",
    Description = "Adds rs1 and rs2, truncating the result to 32 bits and sign-extending to 64 bits.",
    Usage = "addw rd, rs1, rs2")]
public class AddwInstruction : RTypeInstruction
{
    public AddwInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }

    public override void Execute(MachineState s, InstructionData d)
    {
        long res = (long)s.Registers.Read(d.Rs1) + (long)s.Registers.Read(d.Rs2);
        s.Registers.Write(d.Rd, (ulong)(long)(int)res);
    }
}
