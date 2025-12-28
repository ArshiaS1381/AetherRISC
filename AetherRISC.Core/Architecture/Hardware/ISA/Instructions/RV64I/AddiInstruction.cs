using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
namespace AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64I;

[RiscvInstruction("ADDI", InstructionSet.RV64I, RiscvEncodingType.I, 0x13, Funct3 = 0,
    Name = "Add Immediate",
    Description = "Adds the sign-extended 12-bit immediate to register rs1. In RV64, the result is 64-bit.",
    Usage = "addi rd, rs1, imm")]
public class AddiInstruction : ITypeInstruction
{
    public AddiInstruction(int rd, int rs1, int imm) : base(rd, rs1, imm) { }

    public override void Execute(MachineState s, InstructionData d)
    {
        // SimpleRunner sign-extends d.Immediate automatically.
        // We just perform the addition.
        ulong res = s.Registers.Read(d.Rs1) + d.Immediate;
        
        if (s.Config.XLEN == 32) res = (ulong)(uint)res;
        s.Registers.Write(d.Rd, res);
    }
}
