using AetherRISC.Core.Architecture.Hardware.ISA;
using System;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.Pipeline;

namespace AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64F;

[RiscvInstruction("FMIN.S", InstructionSet.RV64F, RiscvEncodingType.R, 0x53, Funct3 = 0, Funct7 = 0x14,
    Name = "Floating-Point Minimum (Single)", 
    Description = "Stores the smaller of single-precision rs1 or rs2 in rd.", 
    Usage = "fmin.s fd, fs1, fs2")]
public class FminSInstruction : RTypeInstruction
{
    public FminSInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }

    public override void Execute(MachineState s, InstructionData d) =>
        s.FRegisters.WriteSingle(d.Rd, Math.Min(s.FRegisters.ReadSingle(d.Rs1), s.FRegisters.ReadSingle(d.Rs2)));

    public override void Compute(MachineState state, ulong rs1Val, ulong rs2Val, PipelineBuffers buffers)
    {
        float v1 = state.FRegisters.ReadSingle(this.Rs1);
        float v2 = state.FRegisters.ReadSingle(this.Rs2);
        float res = Math.Min(v1, v2);
        buffers.ExecuteMemory.AluResult = 0xFFFFFFFF00000000 | (ulong)BitConverter.SingleToUInt32Bits(res);
    }
}
