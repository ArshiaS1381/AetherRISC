using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.Pipeline;

namespace AetherRISC.Core.Architecture.Hardware.ISA.Extensions.B.Zbb;

[RiscvInstruction("SEXT.B", InstructionSet.Zbb, RiscvEncodingType.ZbbUnary, 0x13, Funct3 = 1, Funct7 = 0x30, Rs2Sel = 4,
    Name = "Sign Extend Byte",
    Description = "Sign-extends the least-significant byte of rs1 to XLEN bits.",
    Usage = "sext.b rd, rs1")]
public class SextBInstruction : RTypeInstruction
{
    public SextBInstruction(int rd, int rs1, int rs2 = 0) : base(rd, rs1, rs2) { }
    
    public override void Execute(MachineState s, InstructionData d) => 
        s.Registers.Write(d.Rd, (ulong)(long)(sbyte)(s.Registers.Read(d.Rs1) & 0xFF));

    public override void Compute(MachineState state, ulong rs1Val, ulong rs2Val, PipelineBuffers buffers)
    {
        buffers.ExecuteMemory.AluResult = (ulong)(long)(sbyte)(rs1Val & 0xFF);
    }
}
