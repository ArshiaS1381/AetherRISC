using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.Pipeline;

namespace AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64I;

[RiscvInstruction("SLTIU", InstructionSet.RV64I, RiscvEncodingType.I, 0x13, Funct3 = 3,
    Name = "Set Less Than Immediate Unsigned",
    Description = "Unsigned comparison between rs1 and the sign-extended 12-bit immediate. If rs1 < imm, stores 1 in rd; otherwise stores 0.",
    Usage = "sltiu rd, rs1, imm")]
public class SltiuInstruction : ITypeInstruction
{
    public SltiuInstruction(int rd, int rs1, int imm) : base(rd, rs1, imm) { }

    public override void Execute(MachineState s, InstructionData d)
    {
        s.Registers.Write(d.Rd, s.Registers.Read(d.Rs1) < (ulong)(long)d.Immediate ? 1ul : 0ul);
    }

    public override void Compute(MachineState state, ulong rs1Val, ulong rs2Val, PipelineBuffers buffers)
    {
        ulong imm = (ulong)(long)buffers.DecodeExecute.Immediate;
        buffers.ExecuteMemory.AluResult = rs1Val < imm ? 1ul : 0ul;
    }
}
