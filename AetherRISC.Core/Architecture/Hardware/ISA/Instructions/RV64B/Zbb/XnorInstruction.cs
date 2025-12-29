using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.Pipeline;

namespace AetherRISC.Core.Architecture.Hardware.ISA.Extensions.B.Zbb;

[RiscvInstruction("XNOR", InstructionSet.Zbb, RiscvEncodingType.R, 0x33, Funct3 = 4, Funct7 = 0x20,
    Name = "Exclusive NOR", 
    Description = "Performs a bitwise XOR between rs1 and rs2, then bitwise inverts the result.", 
    Usage = "xnor rd, rs1, rs2")]
public class XnorInstruction : RTypeInstruction
{
    public XnorInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }

    public override void Execute(MachineState s, InstructionData d)
    {
        ulong res = ~(s.Registers.Read(d.Rs1) ^ s.Registers.Read(d.Rs2));
        if (s.Config.XLEN == 32) res = (ulong)(uint)res;
        s.Registers.Write(d.Rd, res);
    }

    public override void Compute(MachineState state, ulong rs1Val, ulong rs2Val, PipelineBuffers buffers)
    {
        ulong res = ~(rs1Val ^ rs2Val);
        if (state.Config.XLEN == 32) res = (ulong)(uint)res;
        buffers.ExecuteMemory.AluResult = res;
    }
}
