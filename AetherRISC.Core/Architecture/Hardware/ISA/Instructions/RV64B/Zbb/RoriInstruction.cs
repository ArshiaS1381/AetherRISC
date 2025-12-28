using System.Numerics;
using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
namespace AetherRISC.Core.Architecture.Hardware.ISA.Extensions.B.Zbb;

[RiscvInstruction("RORI", InstructionSet.Zbb, RiscvEncodingType.ShiftImm, 0x13, Funct3 = 5, Funct6 = 0x30,
    Name = "Rotate Right Immediate", 
    Description = "Rotates bits of rs1 right by a constant shift amount.", 
    Usage = "rori rd, rs1, shamt")]
public class RoriInstruction : ITypeInstruction
{
    public RoriInstruction(int rd, int rs1, int imm) : base(rd, rs1, imm) { }

    public override void Execute(MachineState s, InstructionData d)
    {
        ulong val = s.Registers.Read(d.Rs1);
        int shamt = d.Imm;
        
        if (s.Config.XLEN == 32)
            s.Registers.Write(d.Rd, (ulong)BitOperations.RotateRight((uint)val, shamt & 31));
        else
            s.Registers.Write(d.Rd, BitOperations.RotateRight(val, shamt & 63));
    }
}
