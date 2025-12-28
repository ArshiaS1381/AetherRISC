using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
namespace AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64I;

[RiscvInstruction("SUB", InstructionSet.RV64I, RiscvEncodingType.R, 0x33, Funct3 = 0, Funct7 = 0x20,
    Name = "Subtract",
    Description = "Subtracts rs2 from rs1 and stores the result in rd.",
    Usage = "sub rd, rs1, rs2")]
public class SubInstruction : RTypeInstruction 
{
    public SubInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }

    public override void Execute(MachineState s, InstructionData d) 
    {
        ulong res = s.Registers.Read(d.Rs1) - s.Registers.Read(d.Rs2);
        if (s.Config.XLEN == 32) res = (ulong)(uint)res;
        s.Registers.Write(d.Rd, res);
    }
}
