using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.Pipeline;

namespace AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64I;

[RiscvInstruction("SRLI", InstructionSet.RV64I, RiscvEncodingType.ShiftImm, 0x13, Funct3 = 5, Funct6 = 0x00,
    Name = "Shift Right Logical Immediate",
    Description = "Shifts register rs1 right by a constant shift amount (shamt). Vacated bits are zero-filled.",
    Usage = "srli rd, rs1, shamt")]
public class SrliInstruction : ITypeInstruction
{
    public SrliInstruction(int rd, int rs1, int imm) : base(rd, rs1, imm) { }

    public override void Execute(MachineState s, InstructionData d)
    {
        s.Registers.Write(d.Rd, s.Registers.Read(d.Rs1) >> (int)(d.Immediate & 0x3F));
    }

    public override void Compute(MachineState state, ulong rs1Val, ulong rs2Val, PipelineBuffers buffers)
    {
        int shamt = buffers.DecodeExecute.Immediate & 0x3F;
        buffers.ExecuteMemory.AluResult = rs1Val >> shamt;
    }
}
