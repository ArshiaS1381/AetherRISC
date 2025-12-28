using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
namespace AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64A;

[RiscvInstruction("SC.W", InstructionSet.RV64A, RiscvEncodingType.R, 0x2F, Funct3 = 2, Funct7 = 0x03)]
public class ScWInstruction : RTypeInstruction
{
    public ScWInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }
    public override void Execute(MachineState s, InstructionData d) {
        uint addr = (uint)s.Registers.Read(d.Rs1);
        bool success = false;
        if (s.LoadReservationAddress.HasValue && s.LoadReservationAddress.Value == addr) {
            s.Memory!.WriteWord(addr, (uint)s.Registers.Read(d.Rs2));
            success = true;
            s.LoadReservationAddress = null;
        }
        s.Registers.Write(d.Rd, success ? 0UL : 1UL);
    }
}

[RiscvInstruction("SC.D", InstructionSet.RV64A, RiscvEncodingType.R, 0x2F, Funct3 = 3, Funct7 = 0x03)]
public class ScDInstruction : RTypeInstruction {
    public ScDInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }
    public override void Execute(MachineState s, InstructionData d) {
        uint addr = (uint)s.Registers.Read(d.Rs1);
        bool success = false;
        if (s.LoadReservationAddress.HasValue && s.LoadReservationAddress.Value == addr) {
            s.Memory!.WriteDouble(addr, s.Registers.Read(d.Rs2));
            success = true;
            s.LoadReservationAddress = null;
        }
        s.Registers.Write(d.Rd, success ? 0UL : 1UL);
    }
}
