using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
namespace AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64I;

[RiscvInstruction("LWU", InstructionSet.RV64I, RiscvEncodingType.I, 0x03, Funct3 = 6,
    Name = "Load Word Unsigned", 
    Description = "Loads a 32-bit value from memory at rs1 + offset, zero-extends it to 64 bits, and stores it in rd.", 
    Usage = "lwu rd, offset(rs1)")]
public class LwuInstruction : ITypeInstruction
{
    public override bool IsLoad => true;

    public LwuInstruction(int rd, int rs1, int imm) : base(rd, rs1, imm) { }

    public override void Execute(MachineState s, InstructionData d)
    {
        uint addr = (uint)((long)s.Registers.Read(d.Rs1) + (long)(int)d.Immediate);
        uint val = s.Memory!.ReadWord(addr);
        
        // Zero-extend 32-bit to 64-bit
        s.Registers.Write(d.Rd, (ulong)val);
    }
}
