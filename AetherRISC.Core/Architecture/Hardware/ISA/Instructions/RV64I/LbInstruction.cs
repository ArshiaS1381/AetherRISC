using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
namespace AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64I;

[RiscvInstruction("LB", InstructionSet.RV64I, RiscvEncodingType.I, 0x03, Funct3 = 0,
    Name = "Load Byte",
    Description = "Loads an 8-bit value from memory, sign-extends it to XLEN, and stores it in rd.",
    Usage = "lb rd, offset(rs1)")]
public class LbInstruction : ITypeInstruction
{
    public override bool IsLoad => true;

    public LbInstruction(int rd, int rs1, int imm) : base(rd, rs1, imm) { }

    public override void Execute(MachineState s, InstructionData d)
    {
        uint addr = (uint)((long)s.Registers.Read(d.Rs1) + (long)(int)d.Immediate);
        byte val = s.Memory!.ReadByte(addr);
        
        // Sign-extend 8-bit to 64-bit
        s.Registers.Write(d.Rd, (ulong)(sbyte)val);
    }
}
