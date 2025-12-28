using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
namespace AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64I;

[RiscvInstruction("SRLIW", InstructionSet.RV64I, RiscvEncodingType.ShiftImm, 0x1B, Funct3 = 5, Funct6 = 0x00,
    Name = "Shift Right Logical Immediate Word",
    Description = "Shifts the lower 32 bits of rs1 right by the immediate shamt, then sign-extends the result to 64 bits.",
    Usage = "srliw rd, rs1, shamt")]
public class SrliwInstruction : ITypeInstruction 
{
    public SrliwInstruction(int rd, int rs1, int imm) : base(rd, rs1, imm) { }

    public override void Execute(MachineState s, InstructionData d) 
    {
        uint v1 = (uint)s.Registers.Read(d.Rs1);
        int shamt = (int)d.Immediate & 0x1F;
        s.Registers.Write(d.Rd, (ulong)(long)(int)(v1 >> shamt));
    }
}
