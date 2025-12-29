using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.Pipeline;

namespace AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64I;

[RiscvInstruction("NOP", InstructionSet.RV64I, RiscvEncodingType.I, 0x13, Funct3 = 0, Name="No Operation", Description="Do nothing", Usage="nop")]
public class NopInstruction : ITypeInstruction
{
    public NopInstruction(int rd=0, int rs1=0, int imm=0) : base(rd, rs1, imm) { }
    
    public override void Execute(MachineState s, InstructionData d) { }

    public override void Compute(MachineState state, ulong rs1Val, ulong rs2Val, PipelineBuffers buffers)
    {
        buffers.ExecuteMemory.AluResult = 0;
    }
}
