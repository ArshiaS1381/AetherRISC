using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
namespace AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64I;

[RiscvInstruction("SRL", InstructionSet.RV64I, RiscvEncodingType.R, 0x33, Funct3 = 5, Funct7 = 0x00,
    Name = "Shift Right Logical",
    Description = "Shifts rs1 right by the amount in rs2. Vacated bits are zero-filled.",
    Usage = "srl rd, rs1, rs2")]
public class SrlInstruction : RTypeInstruction 
{
    public SrlInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }

    public override void Execute(MachineState s, InstructionData d) 
    {
        int shamtMask = (s.Config.XLEN == 32) ? 0x1F : 0x3F;
        int shamt = (int)s.Registers.Read(d.Rs2) & shamtMask;
        ulong res = s.Registers.Read(d.Rs1) >> shamt;
        
        if (s.Config.XLEN == 32) res = (ulong)(uint)res;
        s.Registers.Write(d.Rd, res);
    }
}
