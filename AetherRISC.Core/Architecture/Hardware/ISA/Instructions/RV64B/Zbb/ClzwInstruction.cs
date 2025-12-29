using System.Numerics;
using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.Pipeline;

namespace AetherRISC.Core.Architecture.Hardware.ISA.Extensions.B.Zbb;

[RiscvInstruction("CLZW", InstructionSet.Zbb, RiscvEncodingType.ZbbUnary, 0x1B, Funct3 = 1, Funct7 = 0x30, Rs2Sel = 0,
    Name = "Count Leading Zeros Word",
    Description = "Counts the number of 0 bits at the MSB end of the 32-bit word.",
    Usage = "clzw rd, rs1")]
public class ClzwInstruction : RTypeInstruction
{
    public ClzwInstruction(int rd, int rs1, int rs2 = 0) : base(rd, rs1, rs2) { }

    public override void Execute(MachineState s, InstructionData d)
    {
        ulong val = s.Registers.Read(d.Rs1);
        uint v = (uint)val; 
        ulong res = (uint)BitOperations.LeadingZeroCount(v);
        s.Registers.Write(d.Rd, res);
    }

    public override void Compute(MachineState state, ulong rs1Val, ulong rs2Val, PipelineBuffers buffers)
    {
        uint v = (uint)rs1Val; 
        buffers.ExecuteMemory.AluResult = (ulong)(uint)BitOperations.LeadingZeroCount(v);
    }
}
