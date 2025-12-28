using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
namespace AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64I;

[RiscvInstruction("SLL", InstructionSet.RV64I, RiscvEncodingType.R, 0x33, Funct3 = 1, Funct7 = 0,
    Name = "Shift Left Logical",
    Description = "Shifts the value in rs1 left by the amount in the lower 6 bits (5 bits for RV32) of rs2.",
    Usage = "sll rd, rs1, rs2")]
public class SllInstruction : RTypeInstruction
{
    public SllInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }

    public override void Execute(MachineState s, InstructionData d)
    {
        int shiftMask = (s.Config.XLEN == 32) ? 0x1F : 0x3F;
        int shamt = (int)s.Registers.Read(d.Rs2) & shiftMask;
        
        ulong val = s.Registers.Read(d.Rs1);
        ulong res = val << shamt;
        
        if (s.Config.XLEN == 32) res = (ulong)(uint)res;
        s.Registers.Write(d.Rd, res);
    }
}
