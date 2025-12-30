using System;
using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.Pipeline;

namespace AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64F
{
    [RiscvInstruction("FLW", InstructionSet.RV64F, RiscvEncodingType.I, 0x07, Funct3 = 2)]
    public class FlwInstruction : ITypeInstruction {
        public override bool IsLoad => true;
        public FlwInstruction(int rd, int rs1, int imm) : base(rd, rs1, imm) { }
        public override void Execute(MachineState s, InstructionData d) => s.FRegisters.WriteSingle(d.Rd, BitConverter.Int32BitsToSingle((int)s.Memory!.ReadWord((uint)((long)s.Registers.Read(d.Rs1) + (long)d.Immediate))));
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) => op.AluResult = (ulong)((long)r1 + (long)op.Immediate);
    }

    [RiscvInstruction("FSW", InstructionSet.RV64F, RiscvEncodingType.S, 0x27, Funct3 = 2)]
    public class FswInstruction : STypeInstruction {
        public FswInstruction(int rs1, int rs2, int imm) : base(rs1, rs2, imm) { }
        public override void Execute(MachineState s, InstructionData d) => s.Memory!.WriteWord((uint)((long)s.Registers.Read(d.Rs1) + (long)d.Immediate), (uint)BitConverter.SingleToInt32Bits(s.FRegisters.ReadSingle(d.Rs2)));
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) { op.AluResult = (ulong)((long)r1 + (long)op.Immediate); op.StoreValue = (uint)BitConverter.SingleToInt32Bits(s.FRegisters.ReadSingle(Rs2)); }
    }

    

    [RiscvInstruction("FADD.S", InstructionSet.RV64F, RiscvEncodingType.R, 0x53, Funct3 = 7, Funct7 = 0x00)]
    public class FaddSInstruction : RTypeInstruction {
        public FaddSInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }
        public override void Execute(MachineState s, InstructionData d) => s.FRegisters.WriteSingle(d.Rd, s.FRegisters.ReadSingle(d.Rs1) + s.FRegisters.ReadSingle(d.Rs2));
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) => op.AluResult = 0xFFFFFFFF00000000 | (ulong)BitConverter.SingleToUInt32Bits(s.FRegisters.ReadSingle(Rs1) + s.FRegisters.ReadSingle(Rs2));
    }

    [RiscvInstruction("FSUB.S", InstructionSet.RV64F, RiscvEncodingType.R, 0x53, Funct3 = 7, Funct7 = 0x04)]
    public class FsubSInstruction : RTypeInstruction {
        public FsubSInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }
        public override void Execute(MachineState s, InstructionData d) => s.FRegisters.WriteSingle(d.Rd, s.FRegisters.ReadSingle(d.Rs1) - s.FRegisters.ReadSingle(d.Rs2));
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) => op.AluResult = 0xFFFFFFFF00000000 | (ulong)BitConverter.SingleToUInt32Bits(s.FRegisters.ReadSingle(Rs1) - s.FRegisters.ReadSingle(Rs2));
    }

    [RiscvInstruction("FMUL.S", InstructionSet.RV64F, RiscvEncodingType.R, 0x53, Funct3 = 7, Funct7 = 0x08)]
    public class FmulSInstruction : RTypeInstruction {
        public FmulSInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }
        public override void Execute(MachineState s, InstructionData d) => s.FRegisters.WriteSingle(d.Rd, s.FRegisters.ReadSingle(d.Rs1) * s.FRegisters.ReadSingle(d.Rs2));
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) => op.AluResult = 0xFFFFFFFF00000000 | (ulong)BitConverter.SingleToUInt32Bits(s.FRegisters.ReadSingle(Rs1) * s.FRegisters.ReadSingle(Rs2));
    }

    [RiscvInstruction("FDIV.S", InstructionSet.RV64F, RiscvEncodingType.R, 0x53, Funct3 = 7, Funct7 = 0x0C)]
    public class FdivSInstruction : RTypeInstruction {
        public FdivSInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }
        public override void Execute(MachineState s, InstructionData d) => s.FRegisters.WriteSingle(d.Rd, s.FRegisters.ReadSingle(d.Rs1) / s.FRegisters.ReadSingle(d.Rs2));
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) => op.AluResult = 0xFFFFFFFF00000000 | (ulong)BitConverter.SingleToUInt32Bits(s.FRegisters.ReadSingle(Rs1) / s.FRegisters.ReadSingle(Rs2));
    }

    [RiscvInstruction("FSQRT.S", InstructionSet.RV64F, RiscvEncodingType.R, 0x53, Funct3 = 7, Funct7 = 0x2C)]
    public class FsqrtSInstruction : RTypeInstruction {
        public FsqrtSInstruction(int rd, int rs1, int rs2 = 0) : base(rd, rs1, rs2) { }
        public override void Execute(MachineState s, InstructionData d) => s.FRegisters.WriteSingle(d.Rd, MathF.Sqrt(s.FRegisters.ReadSingle(d.Rs1)));
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) => op.AluResult = 0xFFFFFFFF00000000 | (ulong)BitConverter.SingleToUInt32Bits(MathF.Sqrt(s.FRegisters.ReadSingle(Rs1)));
    }

    [RiscvInstruction("FSGNJ.S", InstructionSet.RV64F, RiscvEncodingType.R, 0x53, Funct3 = 0, Funct7 = 0x10)]
    public class FsgnjSInstruction : RTypeInstruction {
        public FsgnjSInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }
        public override void Execute(MachineState s, InstructionData d) {
            uint b1 = BitConverter.SingleToUInt32Bits(s.FRegisters.ReadSingle(d.Rs1));
            uint b2 = BitConverter.SingleToUInt32Bits(s.FRegisters.ReadSingle(d.Rs2));
            s.FRegisters.WriteSingle(d.Rd, BitConverter.UInt32BitsToSingle((b1 & 0x7FFFFFFF) | (b2 & 0x80000000)));
        }
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) {
            uint b1 = BitConverter.SingleToUInt32Bits(s.FRegisters.ReadSingle(Rs1));
            uint b2 = BitConverter.SingleToUInt32Bits(s.FRegisters.ReadSingle(Rs2));
            op.AluResult = 0xFFFFFFFF00000000 | ((b1 & 0x7FFFFFFF) | (b2 & 0x80000000));
        }
    }

    [RiscvInstruction("FSGNJN.S", InstructionSet.RV64F, RiscvEncodingType.R, 0x53, Funct3 = 1, Funct7 = 0x10)]
    public class FsgnjnSInstruction : RTypeInstruction {
        public FsgnjnSInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }
        public override void Execute(MachineState s, InstructionData d) {
            uint b1 = BitConverter.SingleToUInt32Bits(s.FRegisters.ReadSingle(d.Rs1));
            uint b2 = BitConverter.SingleToUInt32Bits(s.FRegisters.ReadSingle(d.Rs2));
            s.FRegisters.WriteSingle(d.Rd, BitConverter.UInt32BitsToSingle((b1 & 0x7FFFFFFF) | (~b2 & 0x80000000)));
        }
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) {
            uint b1 = BitConverter.SingleToUInt32Bits(s.FRegisters.ReadSingle(Rs1));
            uint b2 = BitConverter.SingleToUInt32Bits(s.FRegisters.ReadSingle(Rs2));
            op.AluResult = 0xFFFFFFFF00000000 | ((b1 & 0x7FFFFFFF) | (~b2 & 0x80000000));
        }
    }

    [RiscvInstruction("FSGNJX.S", InstructionSet.RV64F, RiscvEncodingType.R, 0x53, Funct3 = 2, Funct7 = 0x10)]
    public class FsgnjxSInstruction : RTypeInstruction {
        public FsgnjxSInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }
        public override void Execute(MachineState s, InstructionData d) {
            uint b1 = BitConverter.SingleToUInt32Bits(s.FRegisters.ReadSingle(d.Rs1));
            uint b2 = BitConverter.SingleToUInt32Bits(s.FRegisters.ReadSingle(d.Rs2));
            s.FRegisters.WriteSingle(d.Rd, BitConverter.UInt32BitsToSingle(b1 ^ (b2 & 0x80000000)));
        }
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) {
            uint b1 = BitConverter.SingleToUInt32Bits(s.FRegisters.ReadSingle(Rs1));
            uint b2 = BitConverter.SingleToUInt32Bits(s.FRegisters.ReadSingle(Rs2));
            op.AluResult = 0xFFFFFFFF00000000 | (b1 ^ (b2 & 0x80000000));
        }
    }

    [RiscvInstruction("FMIN.S", InstructionSet.RV64F, RiscvEncodingType.R, 0x53, Funct3 = 0, Funct7 = 0x14)]
    public class FminSInstruction : RTypeInstruction {
        public FminSInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }
        public override void Execute(MachineState s, InstructionData d) => s.FRegisters.WriteSingle(d.Rd, Math.Min(s.FRegisters.ReadSingle(d.Rs1), s.FRegisters.ReadSingle(d.Rs2)));
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) => op.AluResult = 0xFFFFFFFF00000000 | (ulong)BitConverter.SingleToUInt32Bits(Math.Min(s.FRegisters.ReadSingle(Rs1), s.FRegisters.ReadSingle(Rs2)));
    }

    [RiscvInstruction("FMAX.S", InstructionSet.RV64F, RiscvEncodingType.R, 0x53, Funct3 = 1, Funct7 = 0x14)]
    public class FmaxSInstruction : RTypeInstruction {
        public FmaxSInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }
        public override void Execute(MachineState s, InstructionData d) => s.FRegisters.WriteSingle(d.Rd, Math.Max(s.FRegisters.ReadSingle(d.Rs1), s.FRegisters.ReadSingle(d.Rs2)));
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) => op.AluResult = 0xFFFFFFFF00000000 | (ulong)BitConverter.SingleToUInt32Bits(Math.Max(s.FRegisters.ReadSingle(Rs1), s.FRegisters.ReadSingle(Rs2)));
    }

    [RiscvInstruction("FCVT.W.S", InstructionSet.RV64F, RiscvEncodingType.R, 0x53, Funct3 = 7, Funct7 = 0x60)]
    public class FcvtWSInstruction : RTypeInstruction {
        public FcvtWSInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }
        public override void Execute(MachineState s, InstructionData d) => s.Registers.Write(d.Rd, (ulong)(long)(int)s.FRegisters.ReadSingle(d.Rs1));
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) => op.AluResult = (ulong)(long)(int)s.FRegisters.ReadSingle(Rs1);
    }

    [RiscvInstruction("FCVT.S.W", InstructionSet.RV64F, RiscvEncodingType.R, 0x53, Funct3 = 7, Funct7 = 0x68)]
    public class FcvtSWInstruction : RTypeInstruction {
        public FcvtSWInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }
        public override void Execute(MachineState s, InstructionData d) => s.FRegisters.WriteSingle(d.Rd, (float)(int)s.Registers.Read(d.Rs1));
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) => op.AluResult = 0xFFFFFFFF00000000 | (ulong)BitConverter.SingleToUInt32Bits((float)(int)r1);
    }

    [RiscvInstruction("FMV.X.W", InstructionSet.RV64F, RiscvEncodingType.R, 0x53, Funct3 = 0, Funct7 = 0x70)]
    public class FmvXWInstruction : RTypeInstruction {
        public FmvXWInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }
        public override void Execute(MachineState s, InstructionData d) => s.Registers.Write(d.Rd, (ulong)(long)BitConverter.SingleToInt32Bits(s.FRegisters.ReadSingle(d.Rs1)));
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) => op.AluResult = (ulong)(long)BitConverter.SingleToInt32Bits(s.FRegisters.ReadSingle(Rs1));
    }

    [RiscvInstruction("FMV.W.X", InstructionSet.RV64F, RiscvEncodingType.R, 0x53, Funct3 = 0, Funct7 = 0x78)]
    public class FmvWXInstruction : RTypeInstruction {
        public FmvWXInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }
        public override void Execute(MachineState s, InstructionData d) => s.FRegisters.WriteSingle(d.Rd, BitConverter.Int32BitsToSingle((int)s.Registers.Read(d.Rs1)));
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) => op.AluResult = 0xFFFFFFFF00000000 | (r1 & 0xFFFFFFFF);
    }

    [RiscvInstruction("FEQ.S", InstructionSet.RV64F, RiscvEncodingType.R, 0x53, Funct3 = 2, Funct7 = 0x50)]
    public class FeqSInstruction : RTypeInstruction {
        public FeqSInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }
        public override void Execute(MachineState s, InstructionData d) => s.Registers.Write(d.Rd, s.FRegisters.ReadSingle(d.Rs1) == s.FRegisters.ReadSingle(d.Rs2) ? 1UL : 0UL);
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) => op.AluResult = s.FRegisters.ReadSingle(Rs1) == s.FRegisters.ReadSingle(Rs2) ? 1UL : 0UL;
    }

    [RiscvInstruction("FLT.S", InstructionSet.RV64F, RiscvEncodingType.R, 0x53, Funct3 = 1, Funct7 = 0x50)]
    public class FltSInstruction : RTypeInstruction {
        public FltSInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }
        public override void Execute(MachineState s, InstructionData d) => s.Registers.Write(d.Rd, s.FRegisters.ReadSingle(d.Rs1) < s.FRegisters.ReadSingle(d.Rs2) ? 1UL : 0UL);
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) => op.AluResult = s.FRegisters.ReadSingle(Rs1) < s.FRegisters.ReadSingle(Rs2) ? 1UL : 0UL;
    }

    [RiscvInstruction("FLE.S", InstructionSet.RV64F, RiscvEncodingType.R, 0x53, Funct3 = 0, Funct7 = 0x50)]
    public class FleSInstruction : RTypeInstruction {
        public FleSInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }
        public override void Execute(MachineState s, InstructionData d) => s.Registers.Write(d.Rd, s.FRegisters.ReadSingle(d.Rs1) <= s.FRegisters.ReadSingle(d.Rs2) ? 1UL : 0UL);
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) => op.AluResult = s.FRegisters.ReadSingle(Rs1) <= s.FRegisters.ReadSingle(Rs2) ? 1UL : 0UL;
    }

    [RiscvInstruction("FCLASS.S", InstructionSet.RV64F, RiscvEncodingType.R, 0x53, Funct3 = 1, Funct7 = 0x70)]
    public class FclassSInstruction : RTypeInstruction {
        public FclassSInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }
        public override void Execute(MachineState s, InstructionData d) => s.Registers.Write(d.Rd, Classify(s.FRegisters.ReadSingle(d.Rs1)));
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) => op.AluResult = Classify(s.FRegisters.ReadSingle(Rs1));
        private ulong Classify(float val) {
            uint bits = BitConverter.SingleToUInt32Bits(val);
            bool sign = (bits >> 31) != 0;
            int exp = (int)((bits >> 23) & 0xFF);
            uint frac = bits & 0x7FFFFF;
            if (exp == 0xFF) return frac == 0 ? (sign ? 1ul : 128ul) : ((frac & 0x400000) != 0 ? 512ul : 256ul);
            if (exp == 0) return frac == 0 ? (sign ? 8ul : 16ul) : (sign ? 4ul : 32ul);
            return sign ? 2ul : 64ul;
        }
    }
}
