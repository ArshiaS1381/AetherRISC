using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.Pipeline;

namespace AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64I;

[RiscvInstruction("SLLIW", InstructionSet.RV64I, RiscvEncodingType.ShiftImm, 0x1B, Funct3 = 1, Funct6 = 0x00,
    Name = "Shift Left Logical Immediate Word",
    Description = "Shifts the lower 32 bits of rs1 left by a constant shift amount, then sign-extends the result to 64 bits.",
    Usage = "slliw rd, rs1, shamt")]
public class SlliwInstruction : ITypeInstruction
{
    public SlliwInstruction(int rd, int rs1, int imm) : base(rd, rs1, imm) { }

    public override void Execute(MachineState s, InstructionData d)
    {
        int v1 = (int)s.Registers.Read(d.Rs1);
        int shamt = (int)d.Immediate & 0x1F;
        s.Registers.Write(d.Rd, (ulong)(long)(v1 << shamt));
    }

    public override void Compute(MachineState state, ulong rs1Val, ulong rs2Val, PipelineBuffers buffers)
    {
        int v1 = (int)rs1Val;
        int shamt = buffers.DecodeExecute.Immediate & 0x1F;
        buffers.ExecuteMemory.AluResult = (ulong)(long)(v1 << shamt);
    }
}
