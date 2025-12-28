using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
namespace AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64D;

[RiscvInstruction("FADD.D", InstructionSet.RV64D, RiscvEncodingType.R, 0x53, Funct3 = 7, Funct7 = 0x01,
    Name = "Floating-Point Add (Double)", 
    Description = "Adds two double-precision values.", 
    Usage = "fadd.d fd, fs1, fs2")]
public class FaddDInstruction : RTypeInstruction
{
    public FaddDInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }

    public override void Execute(MachineState s, InstructionData d)
    {
        double v1 = s.FRegisters.ReadDouble(d.Rs1);
        double v2 = s.FRegisters.ReadDouble(d.Rs2);
        s.FRegisters.WriteDouble(d.Rd, v1 + v2);
    }
}
