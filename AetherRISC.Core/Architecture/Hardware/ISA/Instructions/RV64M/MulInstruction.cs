using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
namespace AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64M;

[RiscvInstruction("MUL", InstructionSet.RV64M, RiscvEncodingType.R, 0x33, Funct3 = 0, Funct7 = 1,
    Name = "Multiply", 
    Description = "Multiplies rs1 and rs2 and stores the lower XLEN bits of the result in rd.", 
    Usage = "mul rd, rs1, rs2")]
public class MulInstruction : RTypeInstruction
{
    public MulInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }

    public override void Execute(MachineState s, InstructionData d)
    {
        long v1 = (long)s.Registers.Read(d.Rs1);
        long v2 = (long)s.Registers.Read(d.Rs2);
        ulong res = unchecked((ulong)(v1 * v2));
        
        if (s.Config.XLEN == 32) res = (ulong)(uint)res;
        s.Registers.Write(d.Rd, res);
    }
}
