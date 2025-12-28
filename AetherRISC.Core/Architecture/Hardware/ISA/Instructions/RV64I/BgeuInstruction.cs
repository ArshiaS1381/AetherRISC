using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
namespace AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64I;

[RiscvInstruction("BGEU", InstructionSet.RV64I, RiscvEncodingType.B, 0x63, Funct3 = 7,
    Name = "Branch Greater or Equal Unsigned",
    Description = "Branches to the offset if rs1 is greater than or equal to rs2 (unsigned comparison).",
    Usage = "bgeu rs1, rs2, offset")]
public class BgeuInstruction : BTypeInstruction
{
    public BgeuInstruction(int rs1, int rs2, int imm) : base(rs1, rs2, imm) { }

    // Legacy constructor support
    public BgeuInstruction(int rs1, int rs2, int imm, uint dummy) : base(rs1, rs2, imm) { }

    public override void Execute(MachineState s, InstructionData d)
    {
        ulong val1 = s.Registers.Read(d.Rs1);
        ulong val2 = s.Registers.Read(d.Rs2);
        
        if (val1 >= val2)
        {
            s.ProgramCounter = d.PC + (ulong)(long)(int)d.Immediate;
        }
    }
}
