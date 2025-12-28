using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
namespace AetherRISC.Core.Architecture.Hardware.ISA.Extensions.Zifencei;

[RiscvInstruction("FENCE.I", InstructionSet.Zifencei, RiscvEncodingType.I, 0x0F, Funct3 = 1,
    Name = "Instruction Fence", 
    Description = "Flushes the instruction cache and ensures that instruction fetches after the fence see all previous memory writes.", 
    Usage = "fence.i")]
public class FenceIInstruction : ITypeInstruction
{
    public FenceIInstruction(int rd, int rs1, int imm) : base(rd, rs1, imm) { }
    public override void Execute(MachineState s, InstructionData d) { /* Logic handled by cache subsystem if implemented */ }
}
