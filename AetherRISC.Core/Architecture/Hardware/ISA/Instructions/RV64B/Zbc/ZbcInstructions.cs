using System;
using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.Pipeline;

namespace AetherRISC.Core.Architecture.Hardware.ISA.Extensions.B.Zbc
{
    public static class CarrylessMath
    {
        public static (ulong lo, ulong hi) ClmulLoHi(ulong rs1, ulong rs2, int xlen) {
            ulong lo = 0, hi = 0;
            for (int i = 0; i < xlen; i++) {
                if (((rs2 >> i) & 1ul) != 0) { lo ^= (rs1 << i); if (i > 0) hi ^= (rs1 >> (xlen - i)); }
            }
            return (lo, hi);
        }
        public static ulong Clmulr(ulong rs1, ulong rs2, int xlen) {
            ulong res = 0;
            for (int i = 0; i < xlen; i++) { if (((rs2 >> i) & 1ul) != 0) res ^= (rs1 >> (xlen - i - 1)); }
            return res;
        }
    }

    [RiscvInstruction("CLMUL", InstructionSet.Zbc, RiscvEncodingType.R, 0x33, Funct3 = 1, Funct7 = 0x05)]
    public class ClmulInstruction : RTypeInstruction {
        public ClmulInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }
        public override void Execute(MachineState s, InstructionData d) => s.Registers.Write(d.Rd, CarrylessMath.ClmulLoHi(s.Registers.Read(d.Rs1), s.Registers.Read(d.Rs2), s.Config.XLEN).lo);
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) => op.AluResult = CarrylessMath.ClmulLoHi(r1, r2, s.Config.XLEN).lo;
    }

    [RiscvInstruction("CLMULH", InstructionSet.Zbc, RiscvEncodingType.R, 0x33, Funct3 = 3, Funct7 = 0x05)]
    public class ClmulhInstruction : RTypeInstruction {
        public ClmulhInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }
        public override void Execute(MachineState s, InstructionData d) => s.Registers.Write(d.Rd, CarrylessMath.ClmulLoHi(s.Registers.Read(d.Rs1), s.Registers.Read(d.Rs2), s.Config.XLEN).hi);
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) => op.AluResult = CarrylessMath.ClmulLoHi(r1, r2, s.Config.XLEN).hi;
    }

    [RiscvInstruction("CLMULR", InstructionSet.Zbc, RiscvEncodingType.R, 0x33, Funct3 = 2, Funct7 = 0x05)]
    public class ClmulrInstruction : RTypeInstruction {
        public ClmulrInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }
        public override void Execute(MachineState s, InstructionData d) => s.Registers.Write(d.Rd, CarrylessMath.Clmulr(s.Registers.Read(d.Rs1), s.Registers.Read(d.Rs2), s.Config.XLEN));
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) => op.AluResult = CarrylessMath.Clmulr(r1, r2, s.Config.XLEN);
    }
}
