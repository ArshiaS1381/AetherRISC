using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.Pipeline;

namespace AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64D;

[RiscvInstruction("FLT.D", InstructionSet.RV64D, RiscvEncodingType.R, 0x53, Funct3 = 1, Funct7 = 0x51,
    Name = "Less Than (Double)", 
    Description = "Stores 1 in rd if rs1 < rs2.", 
    Usage = "flt.d rd, fs1, fs2")]
public class FltDInstruction : RTypeInstruction
{
    public FltDInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }
    
    public override void Execute(MachineState s, InstructionData d) =>
        s.Registers.Write(d.Rd, s.FRegisters.ReadDouble(d.Rs1) < s.FRegisters.ReadDouble(d.Rs2) ? 1u : 0u);

    public override void Compute(MachineState state, ulong rs1Val, ulong rs2Val, PipelineBuffers buffers)
    {
        double v1 = state.FRegisters.ReadDouble(this.Rs1);
        double v2 = state.FRegisters.ReadDouble(this.Rs2);
        buffers.ExecuteMemory.AluResult = (v1 < v2) ? 1u : 0u;
    }
}
