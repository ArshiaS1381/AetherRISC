using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
namespace AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64I;

[RiscvInstruction("JAL", InstructionSet.RV64I, RiscvEncodingType.J, 0x6F,
    Name = "Jump and Link",
    Description = "Jumps to a PC-relative offset and stores the address of the next instruction (PC+4) in rd.",
    Usage = "jal rd, offset")]
public class JalInstruction : JTypeInstruction 
{
    public JalInstruction(int rd, int imm) : base(rd, imm) { }

    public override void Execute(MachineState s, InstructionData d) 
    {
        // RISC-V JAL stores PC+4 into rd
        s.Registers.Write(d.Rd, d.PC + 4);

        // Update PC with the offset
        s.ProgramCounter = d.PC + (ulong)(long)(int)d.Immediate;
    }
}
