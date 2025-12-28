using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
namespace AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64M;

[RiscvInstruction("MULHSU", InstructionSet.RV64M, RiscvEncodingType.R, 0x33, Funct3 = 2, Funct7 = 1,
    Name = "Multiply High Signed/Unsigned", 
    Description = "Multiplies signed rs1 and unsigned rs2, storing the upper XLEN bits of the result in rd.", 
    Usage = "mulhsu rd, rs1, rs2")]
public class MulhsuInstruction : RTypeInstruction
{
    public MulhsuInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }

    public override void Execute(MachineState s, InstructionData d)
    {
        long a = (long)s.Registers.Read(d.Rs1);
        ulong b = s.Registers.Read(d.Rs2);
        var result = (System.Int128)a * (System.Int128)(long)b;
        if ((long)b < 0) result += (System.Int128)a << 64;
        s.Registers.Write(d.Rd, (ulong)(result >> 64));
    }
}
