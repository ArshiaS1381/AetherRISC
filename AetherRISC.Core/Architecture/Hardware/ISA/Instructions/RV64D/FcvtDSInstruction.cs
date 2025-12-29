using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.Pipeline;
using System;

namespace AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64D;

[RiscvInstruction("FCVT.D.S", InstructionSet.RV64D, RiscvEncodingType.R, 0x53, Funct3 = 7, Funct7 = 0x21,
    Name = "Convert Single to Double", 
    Description = "Converts a single-precision value in rs1 to double-precision in rd.", 
    Usage = "fcvt.d.s fd, fs1")]
public class FcvtDSInstruction : RTypeInstruction
{
    public FcvtDSInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }

    public override void Execute(MachineState s, InstructionData d) =>
        s.FRegisters.WriteDouble(d.Rd, (double)s.FRegisters.ReadSingle(d.Rs1));

    public override void Compute(MachineState state, ulong rs1Val, ulong rs2Val, PipelineBuffers buffers)
    {
        float v1 = state.FRegisters.ReadSingle(this.Rs1);
        buffers.ExecuteMemory.AluResult = BitConverter.DoubleToUInt64Bits((double)v1);
    }
}
