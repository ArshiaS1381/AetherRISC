using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.Pipeline;

namespace AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64I;

[RiscvInstruction("JAL", InstructionSet.RV64I, RiscvEncodingType.J, 0x6F,
    Name = "Jump and Link",
    Description = "Jumps to a PC-relative offset and stores the address of the next instruction (PC+4) in rd.",
    Usage = "jal rd, offset")]
public class JalInstruction : JTypeInstruction 
{
    public JalInstruction(int rd, int imm) : base(rd, imm) { }

    public override void Execute(MachineState s, InstructionData d) 
    {
        s.Registers.Write(d.Rd, d.PC + 4);
        s.ProgramCounter = d.PC + (ulong)(long)(int)d.Immediate;
    }

    public override void Compute(MachineState state, ulong rs1Val, ulong rs2Val, PipelineBuffers buffers)
    {
        buffers.ExecuteMemory.AluResult = buffers.DecodeExecute.PC + 4;
        state.Registers.PC = buffers.DecodeExecute.PC + (ulong)(long)buffers.DecodeExecute.Immediate;
        buffers.ExecuteMemory.BranchTaken = true;
    }
}
