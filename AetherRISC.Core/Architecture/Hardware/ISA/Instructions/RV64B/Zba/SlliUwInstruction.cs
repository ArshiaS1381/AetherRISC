using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.Pipeline;

namespace AetherRISC.Core.Architecture.Hardware.ISA.Extensions.B.Zba;

[RiscvInstruction("SLLI.UW", InstructionSet.Zba, RiscvEncodingType.ShiftImm, 0x1B, Funct3 = 1, Funct6 = 0x02,
    Name = "Shift Left Logical Immediate Unsigned Word", 
    Description = "Zero-extends the lower 32 bits of rs1 and shifts the result left by shamt.", 
    Usage = "slli.uw rd, rs1, shamt")]
public class SlliUwInstruction : ITypeInstruction
{
    public SlliUwInstruction(int rd, int rs1, int shamt) : base(rd, rs1, shamt) { }

    public override void Execute(MachineState s, InstructionData d)
    {
        int shamt = (int)(d.Immediate & 0x3F);
        ulong zextRs1 = s.Registers.Read(d.Rs1) & 0xFFFFFFFFul;
        s.Registers.Write(d.Rd, zextRs1 << shamt);
    }

    public override void Compute(MachineState state, ulong rs1Val, ulong rs2Val, PipelineBuffers buffers)
    {
        int shamt = buffers.DecodeExecute.Immediate & 0x3F;
        ulong zextRs1 = rs1Val & 0xFFFFFFFFul;
        buffers.ExecuteMemory.AluResult = zextRs1 << shamt;
    }
}
