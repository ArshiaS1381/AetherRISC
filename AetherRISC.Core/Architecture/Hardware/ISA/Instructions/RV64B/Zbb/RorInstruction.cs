using System.Numerics;
using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
namespace AetherRISC.Core.Architecture.Hardware.ISA.Extensions.B.Zbb;

[RiscvInstruction("ROR", InstructionSet.Zbb, RiscvEncodingType.R, 0x33, Funct3 = 5, Funct7 = 0x30,
    Name = "Rotate Right", 
    Description = "Rotates bits of rs1 right by the amount in rs2.", 
    Usage = "ror rd, rs1, rs2")]
public class RorInstruction : RTypeInstruction
{
    public RorInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }

    public override void Execute(MachineState s, InstructionData d)
    {
        ulong val = s.Registers.Read(d.Rs1);
        int shamt = (int)s.Registers.Read(d.Rs2);
        
        if (s.Config.XLEN == 32)
            s.Registers.Write(d.Rd, (ulong)BitOperations.RotateRight((uint)val, shamt & 31));
        else
            s.Registers.Write(d.Rd, BitOperations.RotateRight(val, shamt & 63));
    }
}
