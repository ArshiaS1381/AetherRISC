using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
namespace AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64I;

[RiscvInstruction("SW", InstructionSet.RV64I, RiscvEncodingType.S, 0x23, Funct3 = 2,
    Name = "Store Word",
    Description = "Stores the lower 32 bits of rs2 into memory at address rs1 + offset.",
    Usage = "sw rs2, offset(rs1)")]
public class SwInstruction : STypeInstruction
{
    public SwInstruction(int rs1, int rs2, int imm) : base(rs1, rs2, imm) { }

    public override void Execute(MachineState s, InstructionData d)
    {
        uint addr = (uint)((long)s.Registers.Read(d.Rs1) + (long)(int)d.Immediate);
        uint val = (uint)s.Registers.Read(d.Rs2);
        
        s.Memory!.WriteWord(addr, val);
    }
}
