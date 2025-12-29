using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.Pipeline;

namespace AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64I;

[RiscvInstruction("AUIPC", InstructionSet.RV64I, RiscvEncodingType.U, 0x17,
    Name = "Add Upper Immediate to PC",
    Description = "Adds the 20-bit upper immediate to the Program Counter (PC) and stores the result in rd.",
    Usage = "auipc rd, imm")]
public class AuipcInstruction : UTypeInstruction
{
    public AuipcInstruction(int rd, int imm) : base(rd, imm) { }

    public override void Execute(MachineState s, InstructionData d)
    {
        s.Registers.Write(d.Rd, d.PC + (ulong)(long)(int)d.Immediate);
    }

    public override void Compute(MachineState state, ulong rs1Val, ulong rs2Val, PipelineBuffers buffers)
    {
        buffers.ExecuteMemory.AluResult = buffers.DecodeExecute.PC + (ulong)(long)buffers.DecodeExecute.Immediate;
    }
}
