using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.Pipeline;

namespace AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64F;

[RiscvInstruction("FCVT.W.S", InstructionSet.RV64F, RiscvEncodingType.R, 0x53, Funct3 = 7, Funct7 = 0x60,
    Name = "Convert Float to Word (Single)", 
    Description = "Converts single-precision fs1 to signed 32-bit integer in rd.", 
    Usage = "fcvt.w.s rd, fs1")]
public class FcvtWSInstruction : RTypeInstruction
{
    public FcvtWSInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }

    public override void Execute(MachineState s, InstructionData d) =>
        s.Registers.Write(d.Rd, (ulong)(long)(int)s.FRegisters.ReadSingle(d.Rs1));

    public override void Compute(MachineState state, ulong rs1Val, ulong rs2Val, PipelineBuffers buffers)
    {
        float val = state.FRegisters.ReadSingle(this.Rs1);
        buffers.ExecuteMemory.AluResult = (ulong)(long)(int)val;
    }
}
