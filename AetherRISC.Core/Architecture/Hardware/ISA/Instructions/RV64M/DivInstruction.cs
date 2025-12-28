using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
namespace AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64M;

[RiscvInstruction("DIV", InstructionSet.RV64M, RiscvEncodingType.R, 0x33, Funct3 = 4, Funct7 = 1)]
public class DivInstruction : RTypeInstruction
{
    public DivInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }
    public override void Execute(MachineState s, InstructionData d) {
        long v1 = (long)s.Registers.Read(d.Rs1);
        long v2 = (long)s.Registers.Read(d.Rs2);
        if (v2 == 0) s.Registers.Write(d.Rd, ulong.MaxValue);
        else if (v1 == unchecked((long)0x8000000000000000) && v2 == -1) s.Registers.Write(d.Rd, unchecked((ulong)(long)0x8000000000000000));
        else s.Registers.Write(d.Rd, (ulong)(v1 / v2));
    }
}
