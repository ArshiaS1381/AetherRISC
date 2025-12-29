using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.Pipeline;

namespace AetherRISC.Core.Architecture.Hardware.ISA.Extensions.B.Zbb;

[RiscvInstruction("ZEXT.H", InstructionSet.Zbb, RiscvEncodingType.ZbbUnary, 0x13, Funct3 = 1, Funct7 = 0x30, Rs2Sel = 6,
    Name = "Zero Extend Halfword",
    Description = "Zero-extends the least-significant 16 bits of rs1 to XLEN bits.",
    Usage = "zext.h rd, rs1")]
public class ZextHInstruction : RTypeInstruction
{
    public ZextHInstruction(int rd, int rs1, int rs2 = 0) : base(rd, rs1, rs2) { }
    
    public override void Execute(MachineState s, InstructionData d) {
        ulong res = s.Registers.Read(d.Rs1) & 0xFFFF;
        if (s.Config.XLEN == 32) res &= 0xFFFFFFFF;
        s.Registers.Write(d.Rd, res);
    }

    public override void Compute(MachineState state, ulong rs1Val, ulong rs2Val, PipelineBuffers buffers)
    {
        buffers.ExecuteMemory.AluResult = rs1Val & 0xFFFF;
    }
}
