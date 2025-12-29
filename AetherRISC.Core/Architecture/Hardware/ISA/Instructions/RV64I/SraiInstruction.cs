using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.Pipeline;

namespace AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64I;

[RiscvInstruction("SRAI", InstructionSet.RV64I, RiscvEncodingType.ShiftImm, 0x13, Funct3 = 5, Funct6 = 0x10,
    Name = "Shift Right Arithmetic Immediate",
    Description = "Shifts rs1 right by a constant amount. The empty high-order bits are filled with the sign bit from rs1.",
    Usage = "srai rd, rs1, shamt")]
public class SraiInstruction : ITypeInstruction
{
    public SraiInstruction(int rd, int rs1, int imm) : base(rd, rs1, imm) { }

    public override void Execute(MachineState s, InstructionData d)
    {
        s.Registers.Write(d.Rd, (ulong)((long)s.Registers.Read(d.Rs1) >> (int)(d.Immediate & 0x3F)));
    }

    public override void Compute(MachineState state, ulong rs1Val, ulong rs2Val, PipelineBuffers buffers)
    {
        int shamt = buffers.DecodeExecute.Immediate & 0x3F;
        buffers.ExecuteMemory.AluResult = (ulong)((long)rs1Val >> shamt);
    }
}
