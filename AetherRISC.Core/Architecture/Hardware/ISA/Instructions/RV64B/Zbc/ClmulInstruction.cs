using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
namespace AetherRISC.Core.Architecture.Hardware.ISA.Extensions.B.Zbc;

[RiscvInstruction("CLMUL", InstructionSet.Zbc, RiscvEncodingType.R, 0x33, Funct3 = 1, Funct7 = 0x05,
    Name = "Carry-less Multiply Low", 
    Description = "Performs carry-less multiplication and stores the low XLEN bits of the result.", 
    Usage = "clmul rd, rs1, rs2")]
public class ClmulInstruction : RTypeInstruction
{
    public ClmulInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }

    public override void Execute(MachineState s, InstructionData d)
    {
        var (lo, _) = CarrylessMath.ClmulLoHi(s.Registers.Read(d.Rs1), s.Registers.Read(d.Rs2), s.Config.XLEN);
        s.Registers.Write(d.Rd, lo);
    }
}

