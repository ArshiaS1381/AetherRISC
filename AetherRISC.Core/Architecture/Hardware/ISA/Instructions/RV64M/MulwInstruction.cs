using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
namespace AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64M;

[RiscvInstruction("MULW", InstructionSet.RV64M, RiscvEncodingType.R, 0x3B, Funct3 = 0, Funct7 = 1,
    Name = "Multiply Word", 
    Description = "Multiplies the low 32 bits of rs1 and rs2, sign-extending the 32-bit result to 64 bits.", 
    Usage = "mulw rd, rs1, rs2")]
public class MulwInstruction : RTypeInstruction
{
    public MulwInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }

    public override void Execute(MachineState s, InstructionData d)
    {
        int v1 = (int)s.Registers.Read(d.Rs1);
        int v2 = (int)s.Registers.Read(d.Rs2);
        s.Registers.Write(d.Rd, (ulong)(long)(v1 * v2));
    }
}
