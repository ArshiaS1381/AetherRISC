using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
namespace AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64M;

[RiscvInstruction("DIVW", InstructionSet.RV64M, RiscvEncodingType.R, 0x3B, Funct3 = 4, Funct7 = 1)]
public class DivwInstruction : RTypeInstruction
{
    public DivwInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }
    public override void Execute(MachineState s, InstructionData d) {
        int v1 = (int)s.Registers.Read(d.Rs1);
        int v2 = (int)s.Registers.Read(d.Rs2);
        if (v2 == 0) s.Registers.Write(d.Rd, ulong.MaxValue);
        else if (v1 == unchecked((int)0x80000000) && v2 == -1) s.Registers.Write(d.Rd, unchecked((ulong)(long)(int)0x80000000));
        else s.Registers.Write(d.Rd, (ulong)(long)(v1 / v2));
    }
}
