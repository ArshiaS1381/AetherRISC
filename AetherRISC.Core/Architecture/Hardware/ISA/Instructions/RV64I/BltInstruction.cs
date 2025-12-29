using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.Pipeline;

namespace AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64I;

[RiscvInstruction("BLT", InstructionSet.RV64I, RiscvEncodingType.B, 0x63, Funct3 = 4,
    Name = "Branch Less Than",
    Description = "Branches to the offset if rs1 is less than rs2 using signed comparison.",
    Usage = "blt rs1, rs2, offset")]
public class BltInstruction : BTypeInstruction
{
    public BltInstruction(int rs1, int rs2, int imm) : base(rs1, rs2, imm) { }
    public BltInstruction(int rs1, int rs2, int imm, uint dummy) : base(rs1, rs2, imm) { }

    public override void Execute(MachineState s, InstructionData d)
    {
        if ((long)s.Registers.Read(d.Rs1) < (long)s.Registers.Read(d.Rs2))
        {
            s.ProgramCounter = d.PC + (ulong)(long)(int)d.Immediate;
        }
    }

    public override void Compute(MachineState state, ulong rs1Val, ulong rs2Val, PipelineBuffers buffers)
    {
        if ((long)rs1Val < (long)rs2Val)
        {
            state.Registers.PC = buffers.DecodeExecute.PC + (ulong)(long)buffers.DecodeExecute.Immediate;
            buffers.ExecuteMemory.BranchTaken = true;
        }
    }
}
