using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.Pipeline;

namespace AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64M;

[RiscvInstruction("MULW", InstructionSet.RV64M, RiscvEncodingType.R, 0x3B, Funct3 = 0, Funct7 = 1,
    Name = "Multiply Word", 
    Description = "Multiplies the low 32 bits of rs1 and rs2, sign-extending the 32-bit result to 64 bits.", 
    Usage = "mulw rd, rs1, rs2")]
public class MulwInstruction : RTypeInstruction
{
    public MulwInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }

    public override void Execute(MachineState s, InstructionData d) =>
        s.Registers.Write(d.Rd, (ulong)((long)(int)s.Registers.Read(d.Rs1) * (long)(int)s.Registers.Read(d.Rs2)));

    public override void Compute(MachineState state, ulong rs1Val, ulong rs2Val, PipelineBuffers buffers)
    {
        buffers.ExecuteMemory.AluResult = (ulong)((long)(int)rs1Val * (long)(int)rs2Val);
    }
}
