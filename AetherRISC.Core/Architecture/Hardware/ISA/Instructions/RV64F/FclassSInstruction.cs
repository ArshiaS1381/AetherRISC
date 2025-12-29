using AetherRISC.Core.Architecture.Hardware.ISA;
using System;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.Pipeline;

namespace AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64F;

[RiscvInstruction("FCLASS.S", InstructionSet.RV64F, RiscvEncodingType.R, 0x53, Funct3 = 1, Funct7 = 0x70,
    Name = "Floating-Point Classify (Single)", 
    Description = "Examines single-precision rs1 and writes a 10-bit mask to rd identifying the type (NaN, Inf, etc.).", 
    Usage = "fclass.s rd, fs1")]
public class FclassSInstruction : RTypeInstruction
{
    public FclassSInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }

    public override void Execute(MachineState s, InstructionData d) => 
        s.Registers.Write(d.Rd, Classify(s.FRegisters.ReadSingle(d.Rs1)));

    public override void Compute(MachineState state, ulong rs1Val, ulong rs2Val, PipelineBuffers buffers)
    {
        // NOTE: Pipeline controller must forward float regs. Assuming rs1Val contains raw float bits.
        // If your pipeline doesn't support float forwarding yet, this reads from state directly.
        float val = state.FRegisters.ReadSingle(this.Rs1);
        buffers.ExecuteMemory.AluResult = Classify(val);
    }

    private uint Classify(float val)
    {
        uint bits = BitConverter.SingleToUInt32Bits(val);
        bool isNeg = (bits >> 31) != 0;
        uint exponent = (bits >> 23) & 0xFF;
        uint fraction = bits & 0x7FFFFF;

        if (exponent == 0xFF) {
            if (fraction == 0) return isNeg ? 1u << 0 : 1u << 7; // Inf
            return ((fraction & 0x400000) != 0) ? 1u << 9 : 1u << 8; // NaN
        }
        if (exponent == 0) {
            if (fraction == 0) return isNeg ? 1u << 3 : 1u << 4; // Zero
            return isNeg ? 1u << 2 : 1u << 5; // Subnormal
        }
        return isNeg ? 1u << 1 : 1u << 6; // Normal
    }
}
