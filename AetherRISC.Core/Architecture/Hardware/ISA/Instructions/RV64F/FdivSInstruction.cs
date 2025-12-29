using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.Pipeline;
using System;

namespace AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64F;

[RiscvInstruction("FDIV.S", InstructionSet.RV64F, RiscvEncodingType.R, 0x53, Funct3 = 7, Funct7 = 0x0C,
    Name = "Floating-Point Divide (Single)", 
    Description = "Divides single-precision rs1 by rs2.", 
    Usage = "fdiv.s fd, fs1, fs2")]
public class FdivSInstruction : RTypeInstruction
{
    public FdivSInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }

    public override void Execute(MachineState s, InstructionData d) =>
        s.FRegisters.WriteSingle(d.Rd, s.FRegisters.ReadSingle(d.Rs1) / s.FRegisters.ReadSingle(d.Rs2));

    public override void Compute(MachineState state, ulong rs1Val, ulong rs2Val, PipelineBuffers buffers)
    {
        float v1 = state.FRegisters.ReadSingle(this.Rs1);
        float v2 = state.FRegisters.ReadSingle(this.Rs2);
        float res = v1 / v2;
        buffers.ExecuteMemory.AluResult = 0xFFFFFFFF00000000 | (ulong)BitConverter.SingleToUInt32Bits(res);
    }
}
