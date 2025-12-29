using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.Pipeline;

namespace AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64I;

[RiscvInstruction("SLLI", InstructionSet.RV64I, RiscvEncodingType.ShiftImm, 0x13, Funct3 = 1, Funct6 = 0x00,
    Name = "Shift Left Logical Immediate",
    Description = "Shifts the value in rs1 left by a constant shift amount (shamt), storing the result in rd.",
    Usage = "slli rd, rs1, shamt")]
public class SlliInstruction : ITypeInstruction
{
    public SlliInstruction(int rd, int rs1, int imm) : base(rd, rs1, imm) { }

    public override void Execute(MachineState s, InstructionData d)
    {
        s.Registers.Write(d.Rd, s.Registers.Read(d.Rs1) << (int)(d.Immediate & 0x3F));
    }

    public override void Compute(MachineState state, ulong rs1Val, ulong rs2Val, PipelineBuffers buffers)
    {
        // Immediate already decoded into buffers.DecodeExecute.Immediate
        int shamt = (int)(buffers.DecodeExecute.Immediate & 0x3F);
        buffers.ExecuteMemory.AluResult = rs1Val << shamt;
    }
}
