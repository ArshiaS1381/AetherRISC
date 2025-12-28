using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
namespace AetherRISC.Core.Architecture.Hardware.ISA.Extensions.B.Zbs;

[RiscvInstruction("BINV", InstructionSet.Zbs, RiscvEncodingType.R, 0x33, Funct3 = 1, Funct7 = 0x34,
    Name = "Bit Invert",
    Description = "Inverts (toggles) a single bit in rs1 at the index specified by rs2.",
    Usage = "binv rd, rs1, rs2")]
public class BinvInstruction : RTypeInstruction
{
    public BinvInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }
    public override void Execute(MachineState s, InstructionData d) =>
        s.Registers.Write(d.Rd, s.Registers.Read(d.Rs1) ^ (1UL << (int)(s.Registers.Read(d.Rs2) & (s.Config.XLEN == 32 ? 31u : 63u))));
}

[RiscvInstruction("BINVI", InstructionSet.Zbs, RiscvEncodingType.ShiftImm, 0x13, Funct3 = 1, Funct6 = 0x1A,
    Name = "Bit Invert Immediate",
    Description = "Inverts a single bit in rs1 at the index specified by the immediate.",
    Usage = "binvi rd, rs1, shamt")]
public class BinviInstruction : ITypeInstruction
{
    public BinviInstruction(int rd, int rs1, int imm) : base(rd, rs1, imm) { }
    public override void Execute(MachineState s, InstructionData d) =>
        s.Registers.Write(d.Rd, s.Registers.Read(d.Rs1) ^ (1UL << (d.Imm & (s.Config.XLEN == 32 ? 31 : 63))));
}

