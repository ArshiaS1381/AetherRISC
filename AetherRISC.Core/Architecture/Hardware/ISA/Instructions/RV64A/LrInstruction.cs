using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
namespace AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64A;

[RiscvInstruction("LR.W", InstructionSet.RV64A, RiscvEncodingType.R, 0x2F, Funct3 = 2, Funct7 = 0x02)]
public class LrWInstruction : RTypeInstruction
{
    public LrWInstruction(int rd, int rs1, int rs2 = 0) : base(rd, rs1, rs2) { }
    public override void Execute(MachineState s, InstructionData d) {
        uint addr = (uint)s.Registers.Read(d.Rs1);
        s.LoadReservationAddress = addr;
        s.Registers.Write(d.Rd, (ulong)(long)(int)s.Memory!.ReadWord(addr));
    }
}

[RiscvInstruction("LR.D", InstructionSet.RV64A, RiscvEncodingType.R, 0x2F, Funct3 = 3, Funct7 = 0x02)]
public class LrDInstruction : RTypeInstruction
{
    public LrDInstruction(int rd, int rs1, int rs2 = 0) : base(rd, rs1, rs2) { }
    public override void Execute(MachineState s, InstructionData d) {
        uint addr = (uint)s.Registers.Read(d.Rs1);
        s.LoadReservationAddress = addr;
        s.Registers.Write(d.Rd, s.Memory!.ReadDouble(addr));
    }
}
