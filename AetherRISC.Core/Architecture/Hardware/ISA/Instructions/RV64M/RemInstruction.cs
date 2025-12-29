using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.Pipeline;

namespace AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64M;

[RiscvInstruction("REM", InstructionSet.RV64M, RiscvEncodingType.R, 0x33, Funct3 = 6, Funct7 = 1,
    Name = "Remainder Signed",
    Description = "Signed remainder of division.",
    Usage = "rem rd, rs1, rs2")]
public class RemInstruction : RTypeInstruction
{
    public RemInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }

    public override void Execute(MachineState s, InstructionData d) =>
        s.Registers.Write(d.Rd, Calc((long)s.Registers.Read(d.Rs1), (long)s.Registers.Read(d.Rs2)));

    public override void Compute(MachineState state, ulong rs1Val, ulong rs2Val, PipelineBuffers buffers)
    {
        buffers.ExecuteMemory.AluResult = Calc((long)rs1Val, (long)rs2Val);
    }

    private ulong Calc(long v1, long v2)
    {
        if (v2 == 0) return (ulong)v1;
        if (v1 == unchecked((long)0x8000000000000000) && v2 == -1) return 0;
        return (ulong)(v1 % v2);
    }
}
