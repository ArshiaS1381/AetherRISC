using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
namespace AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64M;

[RiscvInstruction("MULHU", InstructionSet.RV64M, RiscvEncodingType.R, 0x33, Funct3 = 3, Funct7 = 1,
    Name = "Multiply High Unsigned", 
    Description = "Multiplies unsigned rs1 and rs2, storing the upper XLEN bits of the result in rd.", 
    Usage = "mulhu rd, rs1, rs2")]
public class MulhuInstruction : RTypeInstruction
{
    public MulhuInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }

    public override void Execute(MachineState s, InstructionData d)
    {
        if (s.Config.XLEN == 32)
        {
            ulong a = s.Registers.Read(d.Rs1) & 0xFFFFFFFF;
            ulong b = s.Registers.Read(d.Rs2) & 0xFFFFFFFF;
            s.Registers.Write(d.Rd, (a * b) >> 32);
        }
        else
        {
            ulong a = s.Registers.Read(d.Rs1);
            ulong b = s.Registers.Read(d.Rs2);
            var result = (System.UInt128)a * (System.UInt128)b;
            s.Registers.Write(d.Rd, (ulong)(result >> 64));
        }
    }
}
