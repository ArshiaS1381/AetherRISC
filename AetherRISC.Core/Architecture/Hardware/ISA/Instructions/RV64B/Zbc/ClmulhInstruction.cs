using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.Pipeline;

namespace AetherRISC.Core.Architecture.Hardware.ISA.Extensions.B.Zbc;

[RiscvInstruction("CLMULH", InstructionSet.Zbc, RiscvEncodingType.R, 0x33, Funct3 = 3, Funct7 = 0x05,
    Name = "Carry-less Multiply High", 
    Description = "Performs carry-less multiplication and stores the high XLEN bits of the result.", 
    Usage = "clmulh rd, rs1, rs2")]
public class ClmulhInstruction : RTypeInstruction
{
    public ClmulhInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }

    public override void Execute(MachineState s, InstructionData d)
    {
        var (_, hi) = CarrylessMath.ClmulLoHi(s.Registers.Read(d.Rs1), s.Registers.Read(d.Rs2), s.Config.XLEN);
        s.Registers.Write(d.Rd, hi);
    }

    public override void Compute(MachineState state, ulong rs1Val, ulong rs2Val, PipelineBuffers buffers)
    {
        var (_, hi) = CarrylessMath.ClmulLoHi(rs1Val, rs2Val, state.Config.XLEN);
        buffers.ExecuteMemory.AluResult = hi;
    }
}
