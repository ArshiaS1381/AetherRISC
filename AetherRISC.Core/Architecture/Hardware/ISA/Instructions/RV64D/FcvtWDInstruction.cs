using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.Pipeline;

namespace AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64D;

[RiscvInstruction("FCVT.W.D", InstructionSet.RV64D, RiscvEncodingType.R, 0x53, Funct3 = 7, Funct7 = 0x61,
    Name = "Convert Double to Word", 
    Description = "Converts a double-precision value in rs1 to a 32-bit signed integer in rd.", 
    Usage = "fcvt.w.d rd, fs1")]
public class FcvtWDInstruction : RTypeInstruction
{
    public FcvtWDInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }

    public override void Execute(MachineState s, InstructionData d) =>
        s.Registers.Write(d.Rd, (ulong)(long)(int)s.FRegisters.ReadDouble(d.Rs1));

    public override void Compute(MachineState state, ulong rs1Val, ulong rs2Val, PipelineBuffers buffers)
    {
        double v1 = state.FRegisters.ReadDouble(this.Rs1);
        buffers.ExecuteMemory.AluResult = (ulong)(long)(int)v1;
    }
}
