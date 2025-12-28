using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
namespace AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64I;

[RiscvInstruction("FENCE", InstructionSet.RV64I, RiscvEncodingType.I, 0x0F, Funct3 = 0,
    Name = "Fence",
    Description = "Orders memory access from devices or hart predecessors to successors. In this simulation, it acts as a No-Op.",
    Usage = "fence pred, succ")]
public class FenceInstruction : ITypeInstruction
{
    public FenceInstruction(int rd, int rs1, int imm) : base(rd, rs1, imm) { }

    public override void Execute(MachineState s, InstructionData d)
    {
        // FENCE orders memory operations. Typically a no-op in simple emulators.
    }
}
