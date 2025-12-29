using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.Pipeline;

namespace AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64M;

[RiscvInstruction("DIV", InstructionSet.RV64M, RiscvEncodingType.R, 0x33, Funct3 = 4, Funct7 = 1,
    Name = "Divide Signed",
    Description = "Performs signed integer division of rs1 by rs2.",
    Usage = "div rd, rs1, rs2")]
public class DivInstruction : RTypeInstruction
{
    public DivInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }

    public override void Execute(MachineState s, InstructionData d) =>
        s.Registers.Write(d.Rd, Calc((long)s.Registers.Read(d.Rs1), (long)s.Registers.Read(d.Rs2)));

    public override void Compute(MachineState state, ulong rs1Val, ulong rs2Val, PipelineBuffers buffers)
    {
        buffers.ExecuteMemory.AluResult = Calc((long)rs1Val, (long)rs2Val);
    }

    private ulong Calc(long v1, long v2)
    {
        if (v2 == 0) return ulong.MaxValue;
        if (v1 == unchecked((long)0x8000000000000000) && v2 == -1) return unchecked((ulong)(long)0x8000000000000000);
        return (ulong)(v1 / v2);
    }
}
