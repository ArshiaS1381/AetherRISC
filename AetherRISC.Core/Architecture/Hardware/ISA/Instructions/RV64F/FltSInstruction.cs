using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.Pipeline;

namespace AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64F;

[RiscvInstruction("FLT.S", InstructionSet.RV64F, RiscvEncodingType.R, 0x53, Funct3 = 1, Funct7 = 0x50,
    Name = "Floating-Point Less Than (Single)", 
    Description = "Writes 1 to integer register rd if single-precision rs1 < rs2, else 0.", 
    Usage = "flt.s rd, fs1, fs2")]
public class FltSInstruction : RTypeInstruction
{
    public FltSInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }

    public override void Execute(MachineState s, InstructionData d) =>
        s.Registers.Write(d.Rd, (s.FRegisters.ReadSingle(d.Rs1) < s.FRegisters.ReadSingle(d.Rs2)) ? 1u : 0u);

    public override void Compute(MachineState state, ulong rs1Val, ulong rs2Val, PipelineBuffers buffers)
    {
        float v1 = state.FRegisters.ReadSingle(this.Rs1);
        float v2 = state.FRegisters.ReadSingle(this.Rs2);
        buffers.ExecuteMemory.AluResult = (v1 < v2) ? 1u : 0u;
    }
}
