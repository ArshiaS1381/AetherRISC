using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.Pipeline;

namespace AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64M;

[RiscvInstruction("MULHU", InstructionSet.RV64M, RiscvEncodingType.R, 0x33, Funct3 = 3, Funct7 = 1,
    Name = "Multiply High Unsigned", 
    Description = "Multiplies unsigned rs1 and rs2, storing the upper XLEN bits of the result in rd.", 
    Usage = "mulhu rd, rs1, rs2")]
public class MulhuInstruction : RTypeInstruction
{
    public MulhuInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }

    public override void Execute(MachineState s, InstructionData d) =>
        s.Registers.Write(d.Rd, Calc(s.Registers.Read(d.Rs1), s.Registers.Read(d.Rs2), s.Config.XLEN));

    public override void Compute(MachineState state, ulong rs1Val, ulong rs2Val, PipelineBuffers buffers)
    {
        buffers.ExecuteMemory.AluResult = Calc(rs1Val, rs2Val, state.Config.XLEN);
    }

    private ulong Calc(ulong a, ulong b, int xlen)
    {
        if (xlen == 32) return ((a & 0xFFFFFFFF) * (b & 0xFFFFFFFF)) >> 32;
        var result = (System.UInt128)a * (System.UInt128)b;
        return (ulong)(result >> 64);
    }
}
