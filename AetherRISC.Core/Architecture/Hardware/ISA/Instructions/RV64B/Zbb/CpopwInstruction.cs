using System.Numerics;
using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.Pipeline;

namespace AetherRISC.Core.Architecture.Hardware.ISA.Extensions.B.Zbb;

[RiscvInstruction("CPOPW", InstructionSet.Zbb, RiscvEncodingType.ZbbUnary, 0x1B, Funct3 = 1, Funct7 = 0x30, Rs2Sel = 2,
    Name = "Count Population Word",
    Description = "Counts the number of set bits (1s) in the lower 32 bits.",
    Usage = "cpopw rd, rs1")]
public class CpopwInstruction : RTypeInstruction
{
    public CpopwInstruction(int rd, int rs1, int rs2 = 0) : base(rd, rs1, rs2) { }

    public override void Execute(MachineState s, InstructionData d)
    {
        ulong val = s.Registers.Read(d.Rs1);
        uint v = (uint)val; 
        ulong res = (uint)BitOperations.PopCount(v);
        s.Registers.Write(d.Rd, res);
    }

    public override void Compute(MachineState state, ulong rs1Val, ulong rs2Val, PipelineBuffers buffers)
    {
        uint v = (uint)rs1Val; 
        buffers.ExecuteMemory.AluResult = (ulong)(uint)BitOperations.PopCount(v);
    }
}
