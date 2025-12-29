using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.Pipeline;

namespace AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64M;

[RiscvInstruction("DIVU", InstructionSet.RV64M, RiscvEncodingType.R, 0x33, Funct3 = 5, Funct7 = 1,
    Name = "Divide Unsigned", 
    Description = "Performs an unsigned integer division of rs1 by rs2, storing the quotient in rd.", 
    Usage = "divu rd, rs1, rs2")]
public class DivuInstruction : RTypeInstruction
{
    public DivuInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }

    public override void Execute(MachineState s, InstructionData d) =>
        s.Registers.Write(d.Rd, Calc(s.Registers.Read(d.Rs1), s.Registers.Read(d.Rs2), s.Config.XLEN));

    public override void Compute(MachineState state, ulong rs1Val, ulong rs2Val, PipelineBuffers buffers)
    {
        buffers.ExecuteMemory.AluResult = Calc(rs1Val, rs2Val, state.Config.XLEN);
    }

    private ulong Calc(ulong v1, ulong v2, int xlen)
    {
        if (v2 == 0) return ulong.MaxValue;
        ulong res = v1 / v2;
        if (xlen == 32) res = (ulong)(uint)res;
        return res;
    }
}
