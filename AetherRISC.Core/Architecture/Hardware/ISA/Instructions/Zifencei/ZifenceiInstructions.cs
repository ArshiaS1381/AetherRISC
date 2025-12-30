using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.Pipeline;

namespace AetherRISC.Core.Architecture.Hardware.ISA.Extensions.Zifencei
{
    [RiscvInstruction("FENCE.I", InstructionSet.Zifencei, RiscvEncodingType.I, 0x0F, Funct3 = 1)]
    public class FenceIInstruction : ITypeInstruction {
        public FenceIInstruction(int rd, int rs1, int imm) : base(rd, rs1, imm) { }
        public override void Execute(MachineState s, InstructionData d) { }
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) { op.AluResult = 0; }
    }
}
