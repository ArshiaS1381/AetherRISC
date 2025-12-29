using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.Pipeline;

namespace AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64M;

[RiscvInstruction("MULH", InstructionSet.RV64M, RiscvEncodingType.R, 0x33, Funct3 = 1, Funct7 = 1,
    Name = "Multiply High Signed", 
    Description = "Multiplies signed rs1 and rs2, storing the upper XLEN bits of the result in rd.", 
    Usage = "mulh rd, rs1, rs2")]
public class MulhInstruction : RTypeInstruction
{
    public MulhInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }

    public override void Execute(MachineState s, InstructionData d) =>
        s.Registers.Write(d.Rd, Calc((long)s.Registers.Read(d.Rs1), (long)s.Registers.Read(d.Rs2), s.Config.XLEN));

    public override void Compute(MachineState state, ulong rs1Val, ulong rs2Val, PipelineBuffers buffers)
    {
        buffers.ExecuteMemory.AluResult = Calc((long)rs1Val, (long)rs2Val, state.Config.XLEN);
    }

    private ulong Calc(long a, long b, int xlen)
    {
        if (xlen == 32) return (ulong)(((int)a * (long)(int)b) >> 32) & 0xFFFFFFFF;
        var result = (System.Int128)a * (System.Int128)b;
        return (ulong)(result >> 64);
    }
}
