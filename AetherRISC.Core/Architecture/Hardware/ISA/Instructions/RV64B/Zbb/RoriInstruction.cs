using System.Numerics;
using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.Pipeline;

namespace AetherRISC.Core.Architecture.Hardware.ISA.Extensions.B.Zbb;

[RiscvInstruction("RORI", InstructionSet.Zbb, RiscvEncodingType.ShiftImm, 0x13, Funct3 = 5, Funct6 = 0x30,
    Name = "Rotate Right Immediate", 
    Description = "Rotates bits of rs1 right by a constant shift amount.", 
    Usage = "rori rd, rs1, shamt")]
public class RoriInstruction : ITypeInstruction
{
    public RoriInstruction(int rd, int rs1, int imm) : base(rd, rs1, imm) { }

    public override void Execute(MachineState s, InstructionData d)
    {
        s.Registers.Write(d.Rd, Calc(s.Registers.Read(d.Rs1), d.Imm, s.Config.XLEN));
    }

    public override void Compute(MachineState state, ulong rs1Val, ulong rs2Val, PipelineBuffers buffers)
    {
        // Pipeline decoder extracts Imm already
        buffers.ExecuteMemory.AluResult = Calc(rs1Val, buffers.DecodeExecute.Immediate, state.Config.XLEN);
    }

    private ulong Calc(ulong val, int shamt, int xlen)
    {
        if (xlen == 32) return (ulong)BitOperations.RotateRight((uint)val, shamt & 31);
        return BitOperations.RotateRight(val, shamt & 63);
    }
}
