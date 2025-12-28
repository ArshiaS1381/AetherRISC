using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
namespace AetherRISC.Core.Architecture.Hardware.ISA.Extensions.B.Zbs;

[RiscvInstruction("BCLR", InstructionSet.Zbs, RiscvEncodingType.R, 0x33, Funct3 = 1, Funct7 = 0x24,
    Name = "Bit Clear",
    Description = "Clears a single bit in rs1 at the index specified by rs2.",
    Usage = "bclr rd, rs1, rs2")]
public class BclrInstruction : RTypeInstruction
{
    public BclrInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }
    public override void Execute(MachineState s, InstructionData d) =>
        s.Registers.Write(d.Rd, s.Registers.Read(d.Rs1) & ~(1UL << (int)(s.Registers.Read(d.Rs2) & (s.Config.XLEN == 32 ? 31u : 63u))));
}

[RiscvInstruction("BCLRI", InstructionSet.Zbs, RiscvEncodingType.ShiftImm, 0x13, Funct3 = 1, Funct6 = 0x12,
    Name = "Bit Clear Immediate",
    Description = "Clears a single bit in rs1 at the index specified by the immediate.",
    Usage = "bclri rd, rs1, shamt")]
public class BclriInstruction : ITypeInstruction
{
    public BclriInstruction(int rd, int rs1, int imm) : base(rd, rs1, imm) { }
    public override void Execute(MachineState s, InstructionData d) =>
        s.Registers.Write(d.Rd, s.Registers.Read(d.Rs1) & ~(1UL << (d.Imm & (s.Config.XLEN == 32 ? 31 : 63))));
}

