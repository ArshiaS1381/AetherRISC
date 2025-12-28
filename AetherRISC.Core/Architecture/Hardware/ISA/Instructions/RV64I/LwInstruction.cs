using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
namespace AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64I;

[RiscvInstruction("LW", InstructionSet.RV64I, RiscvEncodingType.I, 0x03, Funct3 = 2,
    Name = "Load Word", 
    Description = "Loads a 32-bit value from memory at rs1 + offset, sign-extends it to 64 bits, and stores it in rd.", 
    Usage = "lw rd, offset(rs1)")]
public class LwInstruction : ITypeInstruction
{
    public override bool IsLoad => true;

    public LwInstruction(int rd, int rs1, int imm) : base(rd, rs1, imm) { }

    public override void Execute(MachineState s, InstructionData d)
    {
        uint addr = (uint)((long)s.Registers.Read(d.Rs1) + (long)(int)d.Immediate);
        uint val = s.Memory!.ReadWord(addr);
        
        // LW sign-extends the 32-bit loaded value to 64-bit XLEN
        s.Registers.Write(d.Rd, (ulong)(int)val);
    }
}
