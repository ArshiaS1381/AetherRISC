using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
namespace AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64M;

[RiscvInstruction("MULH", InstructionSet.RV64M, RiscvEncodingType.R, 0x33, Funct3 = 1, Funct7 = 1,
    Name = "Multiply High Signed", 
    Description = "Multiplies signed rs1 and rs2, storing the upper XLEN bits of the result in rd.", 
    Usage = "mulh rd, rs1, rs2")]
public class MulhInstruction : RTypeInstruction
{
    public MulhInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }

    public override void Execute(MachineState s, InstructionData d)
    {
        if (s.Config.XLEN == 32)
        {
            long a = (int)s.Registers.Read(d.Rs1);
            long b = (int)s.Registers.Read(d.Rs2);
            s.Registers.Write(d.Rd, (ulong)((a * b) >> 32) & 0xFFFFFFFF);
        }
        else
        {
            long a = (long)s.Registers.Read(d.Rs1);
            long b = (long)s.Registers.Read(d.Rs2);
            var result = (System.Int128)a * (System.Int128)b;
            s.Registers.Write(d.Rd, (ulong)(result >> 64));
        }
    }
}


