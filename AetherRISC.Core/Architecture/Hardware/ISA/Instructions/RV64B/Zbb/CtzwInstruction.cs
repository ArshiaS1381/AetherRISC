using System.Numerics;
using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.Pipeline;

namespace AetherRISC.Core.Architecture.Hardware.ISA.Extensions.B.Zbb;

[RiscvInstruction("CTZW", InstructionSet.Zbb, RiscvEncodingType.ZbbUnary, 0x1B, Funct3 = 1, Funct7 = 0x30, Rs2Sel = 1,
    Name = "Count Trailing Zeros Word",
    Description = "Counts the number of 0 bits at the LSB end of the 32-bit word.",
    Usage = "ctzw rd, rs1")]
public class CtzwInstruction : RTypeInstruction
{
    public CtzwInstruction(int rd, int rs1, int rs2 = 0) : base(rd, rs1, rs2) { }

    public override void Execute(MachineState s, InstructionData d)
    {
        ulong val = s.Registers.Read(d.Rs1);
        uint v = (uint)val; 
        ulong res = (uint)BitOperations.TrailingZeroCount(v);
        s.Registers.Write(d.Rd, res);
    }

    public override void Compute(MachineState state, ulong rs1Val, ulong rs2Val, PipelineBuffers buffers)
    {
        uint v = (uint)rs1Val; 
        buffers.ExecuteMemory.AluResult = (ulong)(uint)BitOperations.TrailingZeroCount(v);
    }
}
