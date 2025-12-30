using System;
using System.Numerics;
using System.Buffers.Binary;
using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.Pipeline;

namespace AetherRISC.Core.Architecture.Hardware.ISA.Extensions.B.Zbb
{
    [RiscvInstruction("CTZ", InstructionSet.Zbb, RiscvEncodingType.ZbbUnary, 0x13, Funct3 = 1, Funct7 = 0x30, Rs2Sel = 1)]
    public class CtzInstruction : RTypeInstruction {
        public CtzInstruction(int rd, int rs1, int rs2 = 0) : base(rd, rs1, rs2) { }
        public override void Execute(MachineState s, InstructionData d) { ulong v = s.Registers.Read(d.Rs1); s.Registers.Write(d.Rd, (s.Config.XLEN == 32) ? (uint)BitOperations.TrailingZeroCount((uint)v) : (ulong)BitOperations.TrailingZeroCount(v)); }
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) => op.AluResult = (s.Config.XLEN == 32) ? (uint)BitOperations.TrailingZeroCount((uint)r1) : (ulong)BitOperations.TrailingZeroCount(r1);
    }

    [RiscvInstruction("CTZW", InstructionSet.Zbb, RiscvEncodingType.ZbbUnary, 0x1B, Funct3 = 1, Funct7 = 0x30, Rs2Sel = 1)]
    public class CtzwInstruction : RTypeInstruction {
        public CtzwInstruction(int rd, int rs1, int rs2 = 0) : base(rd, rs1, rs2) { }
        public override void Execute(MachineState s, InstructionData d) => s.Registers.Write(d.Rd, (uint)BitOperations.TrailingZeroCount((uint)s.Registers.Read(d.Rs1)));
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) => op.AluResult = (ulong)(uint)BitOperations.TrailingZeroCount((uint)r1);
    }

    [RiscvInstruction("CLZ", InstructionSet.Zbb, RiscvEncodingType.ZbbUnary, 0x13, Funct3 = 1, Funct7 = 0x30, Rs2Sel = 0)]
    public class ClzInstruction : RTypeInstruction {
        public ClzInstruction(int rd, int rs1, int rs2 = 0) : base(rd, rs1, rs2) { }
        public override void Execute(MachineState s, InstructionData d) { ulong v = s.Registers.Read(d.Rs1); s.Registers.Write(d.Rd, (s.Config.XLEN == 32) ? (uint)BitOperations.LeadingZeroCount((uint)v) : (ulong)BitOperations.LeadingZeroCount(v)); }
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) => op.AluResult = (s.Config.XLEN == 32) ? (uint)BitOperations.LeadingZeroCount((uint)r1) : (ulong)BitOperations.LeadingZeroCount(r1);
    }

    [RiscvInstruction("CLZW", InstructionSet.Zbb, RiscvEncodingType.ZbbUnary, 0x1B, Funct3 = 1, Funct7 = 0x30, Rs2Sel = 0)]
    public class ClzwInstruction : RTypeInstruction {
        public ClzwInstruction(int rd, int rs1, int rs2 = 0) : base(rd, rs1, rs2) { }
        public override void Execute(MachineState s, InstructionData d) => s.Registers.Write(d.Rd, (uint)BitOperations.LeadingZeroCount((uint)s.Registers.Read(d.Rs1)));
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) => op.AluResult = (ulong)(uint)BitOperations.LeadingZeroCount((uint)r1);
    }

    [RiscvInstruction("CPOP", InstructionSet.Zbb, RiscvEncodingType.ZbbUnary, 0x13, Funct3 = 1, Funct7 = 0x30, Rs2Sel = 2)]
    public class CpopInstruction : RTypeInstruction {
        public CpopInstruction(int rd, int rs1, int rs2 = 0) : base(rd, rs1, rs2) { }
        public override void Execute(MachineState s, InstructionData d) { ulong v = s.Registers.Read(d.Rs1); s.Registers.Write(d.Rd, (s.Config.XLEN == 32) ? (uint)BitOperations.PopCount((uint)v) : (ulong)BitOperations.PopCount(v)); }
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) => op.AluResult = (s.Config.XLEN == 32) ? (uint)BitOperations.PopCount((uint)r1) : (ulong)BitOperations.PopCount(r1);
    }

    [RiscvInstruction("CPOPW", InstructionSet.Zbb, RiscvEncodingType.ZbbUnary, 0x1B, Funct3 = 1, Funct7 = 0x30, Rs2Sel = 2)]
    public class CpopwInstruction : RTypeInstruction {
        public CpopwInstruction(int rd, int rs1, int rs2 = 0) : base(rd, rs1, rs2) { }
        public override void Execute(MachineState s, InstructionData d) => s.Registers.Write(d.Rd, (uint)BitOperations.PopCount((uint)s.Registers.Read(d.Rs1)));
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) => op.AluResult = (ulong)(uint)BitOperations.PopCount((uint)r1);
    }

    [RiscvInstruction("MAX", InstructionSet.Zbb, RiscvEncodingType.R, 0x33, Funct3 = 6, Funct7 = 0x05)]
    public class MaxInstruction : RTypeInstruction {
        public MaxInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }
        public override void Execute(MachineState s, InstructionData d) => s.Registers.Write(d.Rd, (ulong)Math.Max((long)s.Registers.Read(d.Rs1), (long)s.Registers.Read(d.Rs2)));
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) => op.AluResult = (ulong)Math.Max((long)r1, (long)r2);
    }

    [RiscvInstruction("MAXU", InstructionSet.Zbb, RiscvEncodingType.R, 0x33, Funct3 = 7, Funct7 = 0x05)]
    public class MaxuInstruction : RTypeInstruction {
        public MaxuInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }
        public override void Execute(MachineState s, InstructionData d) => s.Registers.Write(d.Rd, Math.Max(s.Registers.Read(d.Rs1), s.Registers.Read(d.Rs2)));
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) => op.AluResult = Math.Max(r1, r2);
    }

    [RiscvInstruction("MIN", InstructionSet.Zbb, RiscvEncodingType.R, 0x33, Funct3 = 4, Funct7 = 0x05)]
    public class MinInstruction : RTypeInstruction {
        public MinInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }
        public override void Execute(MachineState s, InstructionData d) => s.Registers.Write(d.Rd, (ulong)Math.Min((long)s.Registers.Read(d.Rs1), (long)s.Registers.Read(d.Rs2)));
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) => op.AluResult = (ulong)Math.Min((long)r1, (long)r2);
    }

    [RiscvInstruction("MINU", InstructionSet.Zbb, RiscvEncodingType.R, 0x33, Funct3 = 5, Funct7 = 0x05)]
    public class MinuInstruction : RTypeInstruction {
        public MinuInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }
        public override void Execute(MachineState s, InstructionData d) => s.Registers.Write(d.Rd, Math.Min(s.Registers.Read(d.Rs1), s.Registers.Read(d.Rs2)));
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) => op.AluResult = Math.Min(r1, r2);
    }

    [RiscvInstruction("ORC.B", InstructionSet.Zbb, RiscvEncodingType.ShiftImm, 0x13, Funct3 = 5, Funct6 = 0x3A)]
    public class OrcBInstruction : ITypeInstruction {
        public OrcBInstruction(int rd, int rs1, int imm) : base(rd, rs1, imm) { }
        public override void Execute(MachineState s, InstructionData d) => s.Registers.Write(d.Rd, OrcB(s.Registers.Read(d.Rs1), s.Config.XLEN));
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) => op.AluResult = OrcB(r1, s.Config.XLEN);
        private ulong OrcB(ulong v, int xlen) {
            ulong r = 0;
            for (int i = 0; i < xlen / 8; i++) { if (((v >> (i * 8)) & 0xFF) != 0) r |= 0xFFUL << (i * 8); }
            return r;
        }
    }

    [RiscvInstruction("ORN", InstructionSet.Zbb, RiscvEncodingType.R, 0x33, Funct3 = 6, Funct7 = 0x20)]
    public class OrnInstruction : RTypeInstruction {
        public OrnInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }
        public override void Execute(MachineState s, InstructionData d) => s.Registers.Write(d.Rd, s.Registers.Read(d.Rs1) | ~s.Registers.Read(d.Rs2));
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) => op.AluResult = r1 | ~r2;
    }

    [RiscvInstruction("ANDN", InstructionSet.Zbb, RiscvEncodingType.R, 0x33, Funct3 = 7, Funct7 = 0x20)]
    public class AndnInstruction : RTypeInstruction {
        public AndnInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }
        public override void Execute(MachineState s, InstructionData d) => s.Registers.Write(d.Rd, s.Registers.Read(d.Rs1) & ~s.Registers.Read(d.Rs2));
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) => op.AluResult = r1 & ~r2;
    }

    [RiscvInstruction("XNOR", InstructionSet.Zbb, RiscvEncodingType.R, 0x33, Funct3 = 4, Funct7 = 0x20)]
    public class XnorInstruction : RTypeInstruction {
        public XnorInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }
        public override void Execute(MachineState s, InstructionData d) => s.Registers.Write(d.Rd, ~(s.Registers.Read(d.Rs1) ^ s.Registers.Read(d.Rs2)));
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) => op.AluResult = ~(r1 ^ r2);
    }

    [RiscvInstruction("REV8", InstructionSet.Zbb, RiscvEncodingType.ShiftImm, 0x13, Funct3 = 5, Funct6 = 0x3B)]
    public class Rev8Instruction : ITypeInstruction {
        public Rev8Instruction(int rd, int rs1, int imm) : base(rd, rs1, imm) { }
        public override void Execute(MachineState s, InstructionData d) => s.Registers.Write(d.Rd, Rev(s.Registers.Read(d.Rs1), s.Config.XLEN));
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) => op.AluResult = Rev(r1, s.Config.XLEN);
        private ulong Rev(ulong v, int xlen) => xlen == 32 ? BinaryPrimitives.ReverseEndianness((uint)v) : BinaryPrimitives.ReverseEndianness(v);
    }

    [RiscvInstruction("ROL", InstructionSet.Zbb, RiscvEncodingType.R, 0x33, Funct3 = 1, Funct7 = 0x30)]
    public class RolInstruction : RTypeInstruction {
        public RolInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }
        public override void Execute(MachineState s, InstructionData d) => s.Registers.Write(d.Rd, Rol(s.Registers.Read(d.Rs1), (int)s.Registers.Read(d.Rs2), s.Config.XLEN));
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) => op.AluResult = Rol(r1, (int)r2, s.Config.XLEN);
        private ulong Rol(ulong v, int n, int xlen) => xlen == 32 ? BitOperations.RotateLeft((uint)v, n & 31) : BitOperations.RotateLeft(v, n & 63);
    }

    [RiscvInstruction("ROR", InstructionSet.Zbb, RiscvEncodingType.R, 0x33, Funct3 = 5, Funct7 = 0x30)]
    public class RorInstruction : RTypeInstruction {
        public RorInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }
        public override void Execute(MachineState s, InstructionData d) => s.Registers.Write(d.Rd, Ror(s.Registers.Read(d.Rs1), (int)s.Registers.Read(d.Rs2), s.Config.XLEN));
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) => op.AluResult = Ror(r1, (int)r2, s.Config.XLEN);
        private ulong Ror(ulong v, int n, int xlen) => xlen == 32 ? BitOperations.RotateRight((uint)v, n & 31) : BitOperations.RotateRight(v, n & 63);
    }

    [RiscvInstruction("RORI", InstructionSet.Zbb, RiscvEncodingType.ShiftImm, 0x13, Funct3 = 5, Funct6 = 0x30)]
    public class RoriInstruction : ITypeInstruction {
        public RoriInstruction(int rd, int rs1, int imm) : base(rd, rs1, imm) { }
        public override void Execute(MachineState s, InstructionData d) => s.Registers.Write(d.Rd, Ror(s.Registers.Read(d.Rs1), d.Imm, s.Config.XLEN));
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) => op.AluResult = Ror(r1, op.Immediate, s.Config.XLEN);
        private ulong Ror(ulong v, int n, int xlen) => xlen == 32 ? BitOperations.RotateRight((uint)v, n & 31) : BitOperations.RotateRight(v, n & 63);
    }

    [RiscvInstruction("SEXT.B", InstructionSet.Zbb, RiscvEncodingType.ZbbUnary, 0x13, Funct3 = 1, Funct7 = 0x30, Rs2Sel = 4)]
    public class SextBInstruction : RTypeInstruction {
        public SextBInstruction(int rd, int rs1, int rs2 = 0) : base(rd, rs1, rs2) { }
        public override void Execute(MachineState s, InstructionData d) => s.Registers.Write(d.Rd, (ulong)(long)(sbyte)s.Registers.Read(d.Rs1));
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) => op.AluResult = (ulong)(long)(sbyte)r1;
    }

    [RiscvInstruction("SEXT.H", InstructionSet.Zbb, RiscvEncodingType.ZbbUnary, 0x13, Funct3 = 1, Funct7 = 0x30, Rs2Sel = 5)]
    public class SextHInstruction : RTypeInstruction {
        public SextHInstruction(int rd, int rs1, int rs2 = 0) : base(rd, rs1, rs2) { }
        public override void Execute(MachineState s, InstructionData d) => s.Registers.Write(d.Rd, (ulong)(long)(short)s.Registers.Read(d.Rs1));
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) => op.AluResult = (ulong)(long)(short)r1;
    }

    [RiscvInstruction("ZEXT.H", InstructionSet.Zbb, RiscvEncodingType.ZbbUnary, 0x13, Funct3 = 1, Funct7 = 0x30, Rs2Sel = 6)]
    public class ZextHInstruction : RTypeInstruction {
        public ZextHInstruction(int rd, int rs1, int rs2 = 0) : base(rd, rs1, rs2) { }
        public override void Execute(MachineState s, InstructionData d) => s.Registers.Write(d.Rd, s.Registers.Read(d.Rs1) & 0xFFFF);
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) => op.AluResult = r1 & 0xFFFF;
    }
}
