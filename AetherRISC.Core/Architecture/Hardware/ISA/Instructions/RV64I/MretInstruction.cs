using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
namespace AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64I;

[RiscvInstruction("MRET", InstructionSet.RV64I, RiscvEncodingType.R, 0x73, Funct3 = 0, Funct7 = 0x18,
    Name = "Machine-mode Return",
    Description = "Returns from machine-mode trap handler by restoring PC from MEPC CSR (0x341).",
    Usage = "mret")]
public class MretInstruction : RTypeInstruction 
{
    // MRET is encoded as R-type with rs2 = 2
    public override int Rs2 => 2;

    public MretInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }

    public override void Execute(MachineState s, InstructionData d) 
    {
        s.ProgramCounter = s.Csr.Read(0x341); // Restore from MEPC
    }
}
