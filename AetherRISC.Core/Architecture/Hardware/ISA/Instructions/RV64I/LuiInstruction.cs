using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
namespace AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64I;

[RiscvInstruction("LUI", InstructionSet.RV64I, RiscvEncodingType.U, 0x37,
    Name = "Load Upper Immediate",
    Description = "Loads the 20-bit immediate into bits 31:12 of the register and sign-extends the 32-bit result to 64 bits.",
    Usage = "lui rd, imm")]
public class LuiInstruction : UTypeInstruction 
{
    public LuiInstruction(int rd, int imm) : base(rd, imm) { }
    
    public override void Execute(MachineState s, InstructionData d) 
    {
        // In RV64, LUI sign-extends the 32-bit result to 64-bits.
        // d.Immediate is expected to be the pre-shifted 32-bit value (bits 31:12).
        s.Registers.Write(d.Rd, (ulong)(long)(int)d.Immediate); 
    }
}
