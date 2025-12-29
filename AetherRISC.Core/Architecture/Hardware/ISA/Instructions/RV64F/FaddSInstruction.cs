using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.Pipeline;
using System;

namespace AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64F;

[RiscvInstruction("FADD.S", InstructionSet.RV64F, RiscvEncodingType.R, 0x53, Funct3 = 7, Funct7 = 0x00,
    Name = "Floating-Point Add (Single)", 
    Description = "Adds the single-precision floating-point values in rs1 and rs2, storing the result in rd.", 
    Usage = "fadd.s fd, fs1, fs2")]
public class FaddSInstruction : RTypeInstruction
{
    public FaddSInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }

    public override void Execute(MachineState s, InstructionData d)
    {
        float v1 = s.FRegisters.ReadSingle(d.Rs1);
        float v2 = s.FRegisters.ReadSingle(d.Rs2);
        s.FRegisters.WriteSingle(d.Rd, v1 + v2);
    }

    public override void Compute(MachineState state, ulong rs1Val, ulong rs2Val, PipelineBuffers buffers)
    {
        // Decode FRegisters via indices (passed as ulong, needs careful handling in Pipeline Controller!)
        // NOTE: Pipeline controller must forward FPRs for FP instructions. 
        // For now, we assume simple read from state if controller doesn't support FP forwarding yet.
        float v1 = state.FRegisters.ReadSingle(this.Rs1);
        float v2 = state.FRegisters.ReadSingle(this.Rs2);
        float res = v1 + v2;
        
        // Return bits for WB stage to write back
        buffers.ExecuteMemory.AluResult = 0xFFFFFFFF00000000 | (ulong)BitConverter.SingleToUInt32Bits(res);
    }
}
