using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
namespace AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64I;

[RiscvInstruction("SLLW", InstructionSet.RV64I, RiscvEncodingType.R, 0x3B, Funct3 = 1, Funct7 = 0,
    Name = "Shift Left Logical Word",
    Description = "Shifts the lower 32 bits of rs1 left by the amount in the lower 5 bits of rs2, then sign-extends the result to 64 bits.",
    Usage = "sllw rd, rs1, rs2")]
public class SllwInstruction : RTypeInstruction
{
    public SllwInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }

    public override void Execute(MachineState s, InstructionData d)
    {
        int v1 = (int)s.Registers.Read(d.Rs1);
        int shamt = (int)s.Registers.Read(d.Rs2) & 0x1F;
        s.Registers.Write(d.Rd, (ulong)(long)(v1 << shamt));
    }
}
