using System.Numerics;
using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.Pipeline;

namespace AetherRISC.Core.Architecture.Hardware.ISA.Extensions.B.Zbb;

[RiscvInstruction("ROR", InstructionSet.Zbb, RiscvEncodingType.R, 0x33, Funct3 = 5, Funct7 = 0x30,
    Name = "Rotate Right", 
    Description = "Rotates bits of rs1 right by the amount in rs2.", 
    Usage = "ror rd, rs1, rs2")]
public class RorInstruction : RTypeInstruction
{
    public RorInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }

    public override void Execute(MachineState s, InstructionData d)
    {
        s.Registers.Write(d.Rd, Calc(s.Registers.Read(d.Rs1), (int)s.Registers.Read(d.Rs2), s.Config.XLEN));
    }

    public override void Compute(MachineState state, ulong rs1Val, ulong rs2Val, PipelineBuffers buffers)
    {
        buffers.ExecuteMemory.AluResult = Calc(rs1Val, (int)rs2Val, state.Config.XLEN);
    }

    private ulong Calc(ulong val, int shamt, int xlen)
    {
        if (xlen == 32) return (ulong)BitOperations.RotateRight((uint)val, shamt & 31);
        return BitOperations.RotateRight(val, shamt & 63);
    }
}
