using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.Pipeline;

namespace AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64M;

[RiscvInstruction("MULHSU", InstructionSet.RV64M, RiscvEncodingType.R, 0x33, Funct3 = 2, Funct7 = 1,
    Name = "Multiply High Signed/Unsigned", 
    Description = "Multiplies signed rs1 and unsigned rs2, storing the upper XLEN bits of the result in rd.", 
    Usage = "mulhsu rd, rs1, rs2")]
public class MulhsuInstruction : RTypeInstruction
{
    public MulhsuInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }

    public override void Execute(MachineState s, InstructionData d) =>
        s.Registers.Write(d.Rd, Calc((long)s.Registers.Read(d.Rs1), s.Registers.Read(d.Rs2), s.Config.XLEN));

    public override void Compute(MachineState state, ulong rs1Val, ulong rs2Val, PipelineBuffers buffers)
    {
        buffers.ExecuteMemory.AluResult = Calc((long)rs1Val, rs2Val, state.Config.XLEN);
    }

    private ulong Calc(long a, ulong b, int xlen)
    {
        // For RV32, this mixed mode logic would need careful casting, currently omitted for brevity as mainly RV64 focus
        var result = (System.Int128)a * (System.Int128)(long)b;
        if ((long)b < 0) result += (System.Int128)a << 64;
        return (ulong)(result >> 64);
    }
}
