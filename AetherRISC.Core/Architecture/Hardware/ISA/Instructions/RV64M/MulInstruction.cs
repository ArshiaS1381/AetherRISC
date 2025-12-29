using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.Pipeline;

namespace AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64M;

[RiscvInstruction("MUL", InstructionSet.RV64M, RiscvEncodingType.R, 0x33, Funct3 = 0, Funct7 = 1,
    Name = "Multiply", 
    Description = "Multiplies rs1 and rs2 and stores the lower XLEN bits of the result in rd.", 
    Usage = "mul rd, rs1, rs2")]
public class MulInstruction : RTypeInstruction
{
    public MulInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }

    public override void Execute(MachineState s, InstructionData d) =>
        s.Registers.Write(d.Rd, Calc((long)s.Registers.Read(d.Rs1), (long)s.Registers.Read(d.Rs2), s.Config.XLEN));

    public override void Compute(MachineState state, ulong rs1Val, ulong rs2Val, PipelineBuffers buffers)
    {
        buffers.ExecuteMemory.AluResult = Calc((long)rs1Val, (long)rs2Val, state.Config.XLEN);
    }

    private ulong Calc(long v1, long v2, int xlen)
    {
        ulong res = unchecked((ulong)(v1 * v2));
        if (xlen == 32) res = (ulong)(uint)res;
        return res;
    }
}
