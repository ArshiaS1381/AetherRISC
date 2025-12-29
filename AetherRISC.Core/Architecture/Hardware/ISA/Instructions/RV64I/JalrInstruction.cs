using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.Pipeline;

namespace AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64I;

[RiscvInstruction("JALR", InstructionSet.RV64I, RiscvEncodingType.I, 0x67, Funct3 = 0,
    Name = "Jump and Link Register",
    Description = "Jump to address in rs1 + immediate, saving PC+4 to rd.",
    Usage = "jalr rd, rs1, imm")]
public class JalrInstruction : ITypeInstruction
{
    public override bool IsJump => true;

    public JalrInstruction(int rd, int rs1, int imm) : base(rd, rs1, imm) { }

    public override void Execute(MachineState s, InstructionData d)
    {
        ulong returnAddr = d.PC + 4;
        ulong target = s.Registers.Read(d.Rs1) + (ulong)(long)d.Immediate;
        target &= ~1UL;

        if (d.Rd != 0) s.Registers.Write(d.Rd, returnAddr);
        s.ProgramCounter = target;
    }

    public override void Compute(MachineState state, ulong rs1Val, ulong rs2Val, PipelineBuffers buffers)
    {
        buffers.ExecuteMemory.AluResult = buffers.DecodeExecute.PC + 4;
        
        ulong target = (rs1Val + (ulong)(long)buffers.DecodeExecute.Immediate) & ~1UL;
        state.Registers.PC = target;
        buffers.ExecuteMemory.BranchTaken = true;
    }
}
