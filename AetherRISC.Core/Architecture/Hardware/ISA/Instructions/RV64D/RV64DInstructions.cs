using System;
using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.Pipeline;

namespace AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64D
{
    [RiscvInstruction("FMV.D.X", InstructionSet.RV64D, RiscvEncodingType.R, 0x53, Funct3 = 0, Funct7 = 0x79)]
    public class FmvDXInstruction : RTypeInstruction {
        public FmvDXInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }
        public override void Execute(MachineState s, InstructionData d) => s.FRegisters.WriteDouble(d.Rd, BitConverter.Int64BitsToDouble((long)s.Registers.Read(d.Rs1)));
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) => op.AluResult = r1;
    }

    [RiscvInstruction("FMV.X.D", InstructionSet.RV64D, RiscvEncodingType.R, 0x53, Funct3 = 0, Funct7 = 0x71)]
    public class FmvXDInstruction : RTypeInstruction {
        public FmvXDInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }
        public override void Execute(MachineState s, InstructionData d) => s.Registers.Write(d.Rd, (ulong)BitConverter.DoubleToInt64Bits(s.FRegisters.ReadDouble(d.Rs1)));
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) => op.AluResult = (ulong)BitConverter.DoubleToInt64Bits(s.FRegisters.ReadDouble(Rs1)); 
    }

    [RiscvInstruction("FSD", InstructionSet.RV64D, RiscvEncodingType.S, 0x27, Funct3 = 3)]
    public class FsdInstruction : STypeInstruction {
        public FsdInstruction(int rs1, int rs2, int imm) : base(rs1, rs2, imm) { }
        public override void Execute(MachineState s, InstructionData d) {
            uint addr = (uint)((long)s.Registers.Read(d.Rs1) + (long)d.Immediate);
            s.Memory!.WriteDouble(addr, (ulong)BitConverter.DoubleToInt64Bits(s.FRegisters.ReadDouble(d.Rs2)));
        }
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) {
            op.AluResult = (ulong)((long)r1 + (long)op.Immediate);
            op.StoreValue = (ulong)BitConverter.DoubleToInt64Bits(s.FRegisters.ReadDouble(Rs2));
        }
    }

    [RiscvInstruction("FSGNJ.D", InstructionSet.RV64D, RiscvEncodingType.R, 0x53, Funct3 = 0, Funct7 = 0x11)]
    public class FsgnjDInstruction : RTypeInstruction {
        public FsgnjDInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }
        public override void Execute(MachineState s, InstructionData d) {
            ulong b1 = BitConverter.DoubleToUInt64Bits(s.FRegisters.ReadDouble(d.Rs1));
            ulong b2 = BitConverter.DoubleToUInt64Bits(s.FRegisters.ReadDouble(d.Rs2));
            s.FRegisters.WriteDouble(d.Rd, BitConverter.UInt64BitsToDouble((b1 & ~(1UL << 63)) | (b2 & (1UL << 63))));
        }
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) {
            ulong b1 = BitConverter.DoubleToUInt64Bits(s.FRegisters.ReadDouble(Rs1));
            ulong b2 = BitConverter.DoubleToUInt64Bits(s.FRegisters.ReadDouble(Rs2));
            op.AluResult = (b1 & ~(1UL << 63)) | (b2 & (1UL << 63));
        }
    }

    [RiscvInstruction("FSGNJN.D", InstructionSet.RV64D, RiscvEncodingType.R, 0x53, Funct3 = 1, Funct7 = 0x11)]
    public class FsgnjnDInstruction : RTypeInstruction {
        public FsgnjnDInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }
        public override void Execute(MachineState s, InstructionData d) {
            ulong b1 = BitConverter.DoubleToUInt64Bits(s.FRegisters.ReadDouble(d.Rs1));
            ulong b2 = BitConverter.DoubleToUInt64Bits(s.FRegisters.ReadDouble(d.Rs2));
            s.FRegisters.WriteDouble(d.Rd, BitConverter.UInt64BitsToDouble((b1 & ~(1UL << 63)) | (~b2 & (1UL << 63))));
        }
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) {
            ulong b1 = BitConverter.DoubleToUInt64Bits(s.FRegisters.ReadDouble(Rs1));
            ulong b2 = BitConverter.DoubleToUInt64Bits(s.FRegisters.ReadDouble(Rs2));
            op.AluResult = (b1 & ~(1UL << 63)) | (~b2 & (1UL << 63));
        }
    }

    [RiscvInstruction("FSGNJX.D", InstructionSet.RV64D, RiscvEncodingType.R, 0x53, Funct3 = 2, Funct7 = 0x11)]
    public class FsgnjxDInstruction : RTypeInstruction {
        public FsgnjxDInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }
        public override void Execute(MachineState s, InstructionData d) {
            ulong b1 = BitConverter.DoubleToUInt64Bits(s.FRegisters.ReadDouble(d.Rs1));
            ulong b2 = BitConverter.DoubleToUInt64Bits(s.FRegisters.ReadDouble(d.Rs2));
            s.FRegisters.WriteDouble(d.Rd, BitConverter.UInt64BitsToDouble(b1 ^ (b2 & (1UL << 63))));
        }
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) {
            ulong b1 = BitConverter.DoubleToUInt64Bits(s.FRegisters.ReadDouble(Rs1));
            ulong b2 = BitConverter.DoubleToUInt64Bits(s.FRegisters.ReadDouble(Rs2));
            op.AluResult = b1 ^ (b2 & (1UL << 63));
        }
    }

    [RiscvInstruction("FSQRT.D", InstructionSet.RV64D, RiscvEncodingType.R, 0x53, Funct3 = 7, Funct7 = 0x2D)]
    public class FsqrtDInstruction : RTypeInstruction {
        public FsqrtDInstruction(int rd, int rs1, int rs2 = 0) : base(rd, rs1, rs2) { }
        public override void Execute(MachineState s, InstructionData d) => s.FRegisters.WriteDouble(d.Rd, Math.Sqrt(s.FRegisters.ReadDouble(d.Rs1)));
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) => op.AluResult = BitConverter.DoubleToUInt64Bits(Math.Sqrt(s.FRegisters.ReadDouble(Rs1)));
    }

    [RiscvInstruction("FADD.D", InstructionSet.RV64D, RiscvEncodingType.R, 0x53, Funct3 = 7, Funct7 = 0x01)]
    public class FaddDInstruction : RTypeInstruction {
        public FaddDInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }
        public override void Execute(MachineState s, InstructionData d) => s.FRegisters.WriteDouble(d.Rd, s.FRegisters.ReadDouble(d.Rs1) + s.FRegisters.ReadDouble(d.Rs2));
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) => op.AluResult = BitConverter.DoubleToUInt64Bits(s.FRegisters.ReadDouble(Rs1) + s.FRegisters.ReadDouble(Rs2));
    }

    [RiscvInstruction("FSUB.D", InstructionSet.RV64D, RiscvEncodingType.R, 0x53, Funct3 = 7, Funct7 = 0x05)]
    public class FsubDInstruction : RTypeInstruction {
        public FsubDInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }
        public override void Execute(MachineState s, InstructionData d) => s.FRegisters.WriteDouble(d.Rd, s.FRegisters.ReadDouble(d.Rs1) - s.FRegisters.ReadDouble(d.Rs2));
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) => op.AluResult = BitConverter.DoubleToUInt64Bits(s.FRegisters.ReadDouble(Rs1) - s.FRegisters.ReadDouble(Rs2));
    }

    [RiscvInstruction("FMUL.D", InstructionSet.RV64D, RiscvEncodingType.R, 0x53, Funct3 = 7, Funct7 = 0x09)]
    public class FmulDInstruction : RTypeInstruction {
        public FmulDInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }
        public override void Execute(MachineState s, InstructionData d) => s.FRegisters.WriteDouble(d.Rd, s.FRegisters.ReadDouble(d.Rs1) * s.FRegisters.ReadDouble(d.Rs2));
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) => op.AluResult = BitConverter.DoubleToUInt64Bits(s.FRegisters.ReadDouble(Rs1) * s.FRegisters.ReadDouble(Rs2));
    }

    [RiscvInstruction("FDIV.D", InstructionSet.RV64D, RiscvEncodingType.R, 0x53, Funct3 = 7, Funct7 = 0x0D)]
    public class FdivDInstruction : RTypeInstruction {
        public FdivDInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }
        public override void Execute(MachineState s, InstructionData d) => s.FRegisters.WriteDouble(d.Rd, s.FRegisters.ReadDouble(d.Rs1) / s.FRegisters.ReadDouble(d.Rs2));
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) => op.AluResult = BitConverter.DoubleToUInt64Bits(s.FRegisters.ReadDouble(Rs1) / s.FRegisters.ReadDouble(Rs2));
    }

    [RiscvInstruction("FMIN.D", InstructionSet.RV64D, RiscvEncodingType.R, 0x53, Funct3 = 0, Funct7 = 0x15)]
    public class FminDInstruction : RTypeInstruction {
        public FminDInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }
        public override void Execute(MachineState s, InstructionData d) => s.FRegisters.WriteDouble(d.Rd, Math.Min(s.FRegisters.ReadDouble(d.Rs1), s.FRegisters.ReadDouble(d.Rs2)));
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) => op.AluResult = BitConverter.DoubleToUInt64Bits(Math.Min(s.FRegisters.ReadDouble(Rs1), s.FRegisters.ReadDouble(Rs2)));
    }

    [RiscvInstruction("FMAX.D", InstructionSet.RV64D, RiscvEncodingType.R, 0x53, Funct3 = 1, Funct7 = 0x15)]
    public class FmaxDInstruction : RTypeInstruction {
        public FmaxDInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }
        public override void Execute(MachineState s, InstructionData d) => s.FRegisters.WriteDouble(d.Rd, Math.Max(s.FRegisters.ReadDouble(d.Rs1), s.FRegisters.ReadDouble(d.Rs2)));
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) => op.AluResult = BitConverter.DoubleToUInt64Bits(Math.Max(s.FRegisters.ReadDouble(Rs1), s.FRegisters.ReadDouble(Rs2)));
    }

    [RiscvInstruction("FCVT.S.D", InstructionSet.RV64D, RiscvEncodingType.R, 0x53, Funct3 = 7, Funct7 = 0x20)]
    public class FcvtSDInstruction : RTypeInstruction {
        public FcvtSDInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }
        public override void Execute(MachineState s, InstructionData d) => s.FRegisters.WriteSingle(d.Rd, (float)s.FRegisters.ReadDouble(d.Rs1));
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) => op.AluResult = 0xFFFFFFFF00000000 | (ulong)BitConverter.SingleToUInt32Bits((float)s.FRegisters.ReadDouble(Rs1));
    }

    [RiscvInstruction("FCVT.D.S", InstructionSet.RV64D, RiscvEncodingType.R, 0x53, Funct3 = 7, Funct7 = 0x21)]
    public class FcvtDSInstruction : RTypeInstruction {
        public FcvtDSInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }
        public override void Execute(MachineState s, InstructionData d) => s.FRegisters.WriteDouble(d.Rd, (double)s.FRegisters.ReadSingle(d.Rs1));
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) => op.AluResult = BitConverter.DoubleToUInt64Bits((double)s.FRegisters.ReadSingle(Rs1));
    }

    [RiscvInstruction("FEQ.D", InstructionSet.RV64D, RiscvEncodingType.R, 0x53, Funct3 = 2, Funct7 = 0x51)]
    public class FeqDInstruction : RTypeInstruction {
        public FeqDInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }
        public override void Execute(MachineState s, InstructionData d) => s.Registers.Write(d.Rd, (s.FRegisters.ReadDouble(d.Rs1) == s.FRegisters.ReadDouble(d.Rs2)) ? 1UL : 0UL);
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) => op.AluResult = (s.FRegisters.ReadDouble(Rs1) == s.FRegisters.ReadDouble(Rs2)) ? 1UL : 0UL;
    }

    [RiscvInstruction("FLT.D", InstructionSet.RV64D, RiscvEncodingType.R, 0x53, Funct3 = 1, Funct7 = 0x51)]
    public class FltDInstruction : RTypeInstruction {
        public FltDInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }
        public override void Execute(MachineState s, InstructionData d) => s.Registers.Write(d.Rd, (s.FRegisters.ReadDouble(d.Rs1) < s.FRegisters.ReadDouble(d.Rs2)) ? 1UL : 0UL);
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) => op.AluResult = (s.FRegisters.ReadDouble(Rs1) < s.FRegisters.ReadDouble(Rs2)) ? 1UL : 0UL;
    }

    [RiscvInstruction("FLE.D", InstructionSet.RV64D, RiscvEncodingType.R, 0x53, Funct3 = 0, Funct7 = 0x51)]
    public class FleDInstruction : RTypeInstruction {
        public FleDInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }
        public override void Execute(MachineState s, InstructionData d) => s.Registers.Write(d.Rd, (s.FRegisters.ReadDouble(d.Rs1) <= s.FRegisters.ReadDouble(d.Rs2)) ? 1UL : 0UL);
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) => op.AluResult = (s.FRegisters.ReadDouble(Rs1) <= s.FRegisters.ReadDouble(Rs2)) ? 1UL : 0UL;
    }

    [RiscvInstruction("FCLASS.D", InstructionSet.RV64D, RiscvEncodingType.R, 0x53, Funct3 = 1, Funct7 = 0x71)]
    public class FclassDInstruction : RTypeInstruction {
        public FclassDInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }
        public override void Execute(MachineState s, InstructionData d) => s.Registers.Write(d.Rd, Classify(s.FRegisters.ReadDouble(d.Rs1)));
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) => op.AluResult = Classify(s.FRegisters.ReadDouble(Rs1));
        private ulong Classify(double val) {
            ulong bits = BitConverter.DoubleToUInt64Bits(val);
            bool sign = (bits >> 63) != 0;
            int exp = (int)((bits >> 52) & 0x7FF);
            ulong frac = bits & 0xFFFFFFFFFFFFF;
            if (exp == 0x7FF) return frac == 0 ? (sign ? 1ul : 128ul) : ((frac & 0x8000000000000) != 0 ? 512ul : 256ul);
            if (exp == 0) return frac == 0 ? (sign ? 8ul : 16ul) : (sign ? 4ul : 32ul);
            return sign ? 2ul : 64ul;
        }
    }

    [RiscvInstruction("FCVT.W.D", InstructionSet.RV64D, RiscvEncodingType.R, 0x53, Funct3 = 7, Funct7 = 0x61)]
    public class FcvtWDInstruction : RTypeInstruction {
        public FcvtWDInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }
        public override void Execute(MachineState s, InstructionData d) => s.Registers.Write(d.Rd, (ulong)(long)(int)s.FRegisters.ReadDouble(d.Rs1));
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) => op.AluResult = (ulong)(long)(int)s.FRegisters.ReadDouble(Rs1);
    }

    [RiscvInstruction("FCVT.L.D", InstructionSet.RV64D, RiscvEncodingType.R, 0x53, Funct3 = 7, Funct7 = 0x61)]
    public class FcvtLDInstruction : RTypeInstruction {
        public FcvtLDInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }
        public override void Execute(MachineState s, InstructionData d) => s.Registers.Write(d.Rd, (ulong)(long)s.FRegisters.ReadDouble(d.Rs1));
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) => op.AluResult = (ulong)(long)s.FRegisters.ReadDouble(Rs1);
    }

    [RiscvInstruction("FCVT.D.W", InstructionSet.RV64D, RiscvEncodingType.R, 0x53, Funct3 = 7, Funct7 = 0x69)]
    public class FcvtDWInstruction : RTypeInstruction {
        public FcvtDWInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }
        public override void Execute(MachineState s, InstructionData d) => s.FRegisters.WriteDouble(d.Rd, (double)(int)s.Registers.Read(d.Rs1));
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) => op.AluResult = BitConverter.DoubleToUInt64Bits((double)(int)r1);
    }

    [RiscvInstruction("FLD", InstructionSet.RV64D, RiscvEncodingType.I, 0x07, Funct3 = 3)]
    public class FldInstruction : ITypeInstruction {
        public override bool IsLoad => true;
        public FldInstruction(int rd, int rs1, int imm) : base(rd, rs1, imm) { }
        public override void Execute(MachineState s, InstructionData d) => s.FRegisters.WriteDouble(d.Rd, s.Memory!.ReadDouble((uint)((long)s.Registers.Read(d.Rs1) + (long)d.Immediate)));
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) => op.AluResult = (ulong)((long)r1 + (long)op.Immediate);
    }
}
