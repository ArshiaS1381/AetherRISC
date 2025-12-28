using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
namespace AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64D;

[RiscvInstruction("FCVT.W.D", InstructionSet.RV64D, RiscvEncodingType.R, 0x53, Funct3 = 7, Funct7 = 0x61,
    Name = "Convert Double to Word", 
    Description = "Converts a double-precision value in rs1 to a 32-bit signed integer in rd.", 
    Usage = "fcvt.w.d rd, fs1")]
public class FcvtWDInstruction : RTypeInstruction
{
    public FcvtWDInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }

    public override void Execute(MachineState s, InstructionData d)
    {
        double v1 = s.FRegisters.ReadDouble(d.Rs1);
        // Note: Default rounding mode is effectively 'truncate' via cast here. 
        // Real hardware uses RM field or CSR.FRM.
        s.Registers.Write(d.Rd, (ulong)(long)(int)v1); 
    }
}


