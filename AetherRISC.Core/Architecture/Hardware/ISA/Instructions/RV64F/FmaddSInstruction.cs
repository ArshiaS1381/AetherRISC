using System;
using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.Pipeline;

namespace AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64F
{
    [RiscvInstruction("FMADD.S", InstructionSet.RV64F, RiscvEncodingType.R4, 0x43, Funct3 = 0)]
    public class FmaddSInstruction : Instruction {
        public override int Rd { get; } 
        public override int Rs1 { get; } 
        public override int Rs2 { get; } 
        public int Rs3 { get; }

        public FmaddSInstruction(int rd, int rs1, int rs2, int rs3) { 
            Rd = rd; Rs1 = rs1; Rs2 = rs2; Rs3 = rs3; 
        }

        public override void Execute(MachineState s, InstructionData d) {
            float val1 = s.FRegisters.ReadSingle(Rs1);
            float val2 = s.FRegisters.ReadSingle(Rs2);
            float val3 = s.FRegisters.ReadSingle(Rs3);
            s.FRegisters.WriteSingle(Rd, (val1 * val2) + val3);
        }

        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) {
            // Note: In superscalar FP, RS3 typically requires its own forwarding path.
            // For now, we pull from FRegisters. In the future, op should hold 3 register inputs.
            float val1 = s.FRegisters.ReadSingle(Rs1);
            float val2 = s.FRegisters.ReadSingle(Rs2);
            float val3 = s.FRegisters.ReadSingle(Rs3);
            float res = (val1 * val2) + val3;
            op.AluResult = 0xFFFFFFFF00000000 | (ulong)BitConverter.SingleToUInt32Bits(res);
        }
    }
}
