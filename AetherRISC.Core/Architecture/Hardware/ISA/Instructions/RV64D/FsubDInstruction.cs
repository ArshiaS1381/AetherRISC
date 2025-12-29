using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.Pipeline;
using System;

namespace AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64D;

[RiscvInstruction("FSUB.D", InstructionSet.RV64D, RiscvEncodingType.R, 0x53, Funct3 = 7, Funct7 = 0x05)]
public class FsubDInstruction : RTypeInstruction
{
    public FsubDInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }
    
    public override void Execute(MachineState s, InstructionData d) => 
        s.FRegisters.WriteDouble(d.Rd, s.FRegisters.ReadDouble(d.Rs1) - s.FRegisters.ReadDouble(d.Rs2));

    public override void Compute(MachineState state, ulong rs1Val, ulong rs2Val, PipelineBuffers buffers)
    {
        double v1 = state.FRegisters.ReadDouble(this.Rs1);
        double v2 = state.FRegisters.ReadDouble(this.Rs2);
        buffers.ExecuteMemory.AluResult = BitConverter.DoubleToUInt64Bits(v1 - v2);
    }
}
