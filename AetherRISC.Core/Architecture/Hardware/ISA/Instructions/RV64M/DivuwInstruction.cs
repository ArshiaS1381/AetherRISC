using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.Pipeline;

namespace AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64M;

[RiscvInstruction("DIVUW", InstructionSet.RV64M, RiscvEncodingType.R, 0x3B, Funct3 = 5, Funct7 = 1,
    Name = "Divide Word Unsigned", 
    Description = "Divides the lower 32 bits of rs1 by rs2 (unsigned), sign-extending the 32-bit quotient to 64 bits.", 
    Usage = "divuw rd, rs1, rs2")]
public class DivuwInstruction : RTypeInstruction
{
    public DivuwInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }

    public override void Execute(MachineState s, InstructionData d)
    {
        uint v1 = (uint)s.Registers.Read(d.Rs1);
        uint v2 = (uint)s.Registers.Read(d.Rs2);
        s.Registers.Write(d.Rd, v2 == 0 ? ulong.MaxValue : (ulong)(long)(int)(v1 / v2));
    }

    public override void Compute(MachineState state, ulong rs1Val, ulong rs2Val, PipelineBuffers buffers)
    {
        uint v1 = (uint)rs1Val;
        uint v2 = (uint)rs2Val;
        buffers.ExecuteMemory.AluResult = v2 == 0 ? ulong.MaxValue : (ulong)(long)(int)(v1 / v2);
    }
}
