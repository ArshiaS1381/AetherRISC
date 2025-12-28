using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
namespace AetherRISC.Core.Architecture.Hardware.ISA.Extensions.B.Zbb;

[RiscvInstruction("SEXT.B", InstructionSet.Zbb, RiscvEncodingType.ZbbUnary, 0x13, Funct3 = 1, Funct7 = 0x30, Rs2Sel = 4)]
public class SextBInstruction : RTypeInstruction
{
    public SextBInstruction(int rd, int rs1, int rs2 = 0) : base(rd, rs1, rs2) { }
    public override void Execute(MachineState s, InstructionData d) => s.Registers.Write(d.Rd, (ulong)(long)(sbyte)(s.Registers.Read(d.Rs1) & 0xFF));
}
