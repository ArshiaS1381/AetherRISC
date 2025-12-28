using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;

namespace AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RvSystem;

[RiscvInstruction("ECALL", InstructionSet.RV64I, RiscvEncodingType.I, 0x73, Funct3 = 0,
    Name = "Environment Call",
    Description = "Raises an Environment Call exception.",
    Usage = "ecall")]
public class EcallInstruction : ITypeInstruction
{
    // FIX: Force imm to 0 strictly
    public EcallInstruction(int rd, int rs1, int imm) : base(rd, rs1, 0) { }

    public override void Execute(MachineState s, InstructionData d)
    {
        if (s.Host != null) s.Host.HandleEcall(s);
    }
}

[RiscvInstruction("EBREAK", InstructionSet.RV64I, RiscvEncodingType.I, 0x73, Funct3 = 0,
    Name = "Environment Break",
    Description = "Returns control to the debugging environment.",
    Usage = "ebreak")]
public class EbreakInstruction : ITypeInstruction
{
    // FIX: Force imm to 1 strictly to differentiate from ECALL (0)
    public EbreakInstruction(int rd, int rs1, int imm) : base(rd, rs1, 1) { }

    public override void Execute(MachineState s, InstructionData d)
    {
        s.Halted = true; 
        if (s.Host != null) s.Host.HandleBreak(s);
    }
}
