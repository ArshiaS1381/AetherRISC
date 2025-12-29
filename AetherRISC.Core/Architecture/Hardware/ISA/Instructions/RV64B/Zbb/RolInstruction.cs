using System.Numerics;
using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.Pipeline;

namespace AetherRISC.Core.Architecture.Hardware.ISA.Extensions.B.Zbb;

[RiscvInstruction("ROL", InstructionSet.Zbb, RiscvEncodingType.R, 0x33, Funct3 = 1, Funct7 = 0x30,
    Name = "Rotate Left", 
    Description = "Rotates bits of rs1 left by the amount in rs2.", 
    Usage = "rol rd, rs1, rs2")]
public class RolInstruction : RTypeInstruction
{
    public RolInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }

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
        if (xlen == 32) return (ulong)BitOperations.RotateLeft((uint)val, shamt & 31);
        return BitOperations.RotateLeft(val, shamt & 63);
    }
}
