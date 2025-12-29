using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.Pipeline;

namespace AetherRISC.Core.Architecture.Hardware.ISA.Extensions.B.Zbc;

[RiscvInstruction("CLMULR", InstructionSet.Zbc, RiscvEncodingType.R, 0x33, Funct3 = 2, Funct7 = 0x05,
    Name = "Carry-less Multiply Reversed", 
    Description = "Performs carry-less multiplication and stores bits [2*XLEN-2:XLEN-1] of the result.", 
    Usage = "clmulr rd, rs1, rs2")]
public class ClmulrInstruction : RTypeInstruction
{
    public ClmulrInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }

    public override void Execute(MachineState s, InstructionData d) =>
        s.Registers.Write(d.Rd, CarrylessMath.Clmulr(s.Registers.Read(d.Rs1), s.Registers.Read(d.Rs2), s.Config.XLEN));

    public override void Compute(MachineState state, ulong rs1Val, ulong rs2Val, PipelineBuffers buffers)
    {
        buffers.ExecuteMemory.AluResult = CarrylessMath.Clmulr(rs1Val, rs2Val, state.Config.XLEN);
    }
}
