using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
namespace AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64I;

[RiscvInstruction("SRAIW", InstructionSet.RV64I, RiscvEncodingType.ShiftImm, 0x1B, Funct3 = 5, Funct6 = 0x20,
    Name = "Shift Right Arithmetic Immediate Word",
    Description = "Shifts the lower 32 bits of rs1 right by a constant amount. Bits vacated are filled with the 31st bit. Result is sign-extended to 64 bits.",
    Usage = "sraiw rd, rs1, shamt")]
public class SraiwInstruction : ITypeInstruction 
{
    public SraiwInstruction(int rd, int rs1, int imm) : base(rd, rs1, imm) { }

    public override void Execute(MachineState s, InstructionData d) 
    {
        // Extract 32-bit value for arithmetic shift
        int v1 = (int)s.Registers.Read(d.Rs1);
        int shamt = (int)d.Immediate & 0x1F;
        
        // Perform 32-bit shift then cast back to ulong (automatic sign-extension to 64 bits)
        s.Registers.Write(d.Rd, (ulong)(long)(v1 >> shamt));
    }
}
