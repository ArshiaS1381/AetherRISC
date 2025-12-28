using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
namespace AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64I;

[RiscvInstruction("LBU", InstructionSet.RV64I, RiscvEncodingType.I, 0x03, Funct3 = 4,
    Name = "Load Byte Unsigned",
    Description = "Loads an 8-bit value from memory, zero-extends it to XLEN, and stores it in rd.",
    Usage = "lbu rd, offset(rs1)")]
public class LbuInstruction : ITypeInstruction
{
    public override bool IsLoad => true;

    public LbuInstruction(int rd, int rs1, int imm) : base(rd, rs1, imm) { }

    public override void Execute(MachineState s, InstructionData d)
    {
        uint addr = (uint)((long)s.Registers.Read(d.Rs1) + (long)(int)d.Immediate);
        byte val = s.Memory!.ReadByte(addr);
        
        // Zero-extend (implicitly handled by ulong cast)
        s.Registers.Write(d.Rd, (ulong)val);
    }
}
