using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.Pipeline;

namespace AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64M;

[RiscvInstruction("DIVW", InstructionSet.RV64M, RiscvEncodingType.R, 0x3B, Funct3 = 4, Funct7 = 1,
    Name = "Divide Word Signed",
    Description = "Divides the lower 32 bits of rs1 by rs2 (signed).",
    Usage = "divw rd, rs1, rs2")]
public class DivwInstruction : RTypeInstruction
{
    public DivwInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }
    
    public override void Execute(MachineState s, InstructionData d) =>
        s.Registers.Write(d.Rd, Calc((int)s.Registers.Read(d.Rs1), (int)s.Registers.Read(d.Rs2)));

    public override void Compute(MachineState state, ulong rs1Val, ulong rs2Val, PipelineBuffers buffers)
    {
        buffers.ExecuteMemory.AluResult = Calc((int)rs1Val, (int)rs2Val);
    }

    private ulong Calc(int v1, int v2)
    {
        if (v2 == 0) return ulong.MaxValue;
        if (v1 == unchecked((int)0x80000000) && v2 == -1) return unchecked((ulong)(long)(int)0x80000000);
        return (ulong)(long)(v1 / v2);
    }
}
