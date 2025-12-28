using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
namespace AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64I;

[RiscvInstruction("AUIPC", InstructionSet.RV64I, RiscvEncodingType.U, 0x17,
    Name = "Add Upper Immediate to PC",
    Description = "Adds the 20-bit upper immediate to the Program Counter (PC) and stores the result in rd.",
    Usage = "auipc rd, imm")]
public class AuipcInstruction : UTypeInstruction
{
    public AuipcInstruction(int rd, int imm) : base(rd, imm) { }

    public override void Execute(MachineState s, InstructionData d)
    {
        // Immediate is 20 bits, usually pre-shifted by assembler/decoder logic 
        // or passed raw. The attribute system assumes standard U-type decoding logic.
        s.Registers.Write(d.Rd, d.PC + (ulong)(long)(int)d.Immediate);
    }
}
