using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.Pipeline;

namespace AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64M
{
    [RiscvInstruction("MUL", InstructionSet.RV64M, RiscvEncodingType.R, 0x33, Funct3 = 0, Funct7 = 1)]
    public class MulInstruction : RTypeInstruction {
        public MulInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }
        public override void Execute(MachineState s, InstructionData d) => s.Registers.Write(d.Rd, Calc(s.Registers.Read(d.Rs1), s.Registers.Read(d.Rs2), s.Config.XLEN));
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) => op.AluResult = Calc(r1, r2, s.Config.XLEN);
        private ulong Calc(ulong v1, ulong v2, int xlen) { ulong r = unchecked(v1 * v2); return xlen == 32 ? (ulong)(uint)r : r; }
    }

    [RiscvInstruction("MULH", InstructionSet.RV64M, RiscvEncodingType.R, 0x33, Funct3 = 1, Funct7 = 1)]
    public class MulhInstruction : RTypeInstruction {
        public MulhInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }
        public override void Execute(MachineState s, InstructionData d) => s.Registers.Write(d.Rd, Calc((long)s.Registers.Read(d.Rs1), (long)s.Registers.Read(d.Rs2), s.Config.XLEN));
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) => op.AluResult = Calc((long)r1, (long)r2, s.Config.XLEN);
        private ulong Calc(long v1, long v2, int xlen) { if (xlen == 32) return (ulong)(((int)v1 * (long)(int)v2) >> 32) & 0xFFFFFFFF; return (ulong)(((System.Int128)v1 * (System.Int128)v2) >> 64); }
    }

    [RiscvInstruction("MULHSU", InstructionSet.RV64M, RiscvEncodingType.R, 0x33, Funct3 = 2, Funct7 = 1)]
    public class MulhsuInstruction : RTypeInstruction {
        public MulhsuInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }
        public override void Execute(MachineState s, InstructionData d) => s.Registers.Write(d.Rd, Calc((long)s.Registers.Read(d.Rs1), s.Registers.Read(d.Rs2), s.Config.XLEN));
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) => op.AluResult = Calc((long)r1, r2, s.Config.XLEN);
        private ulong Calc(long v1, ulong v2, int xlen) { var res = (System.Int128)v1 * (System.Int128)(long)v2; if ((long)v2 < 0) res += (System.Int128)v1 << 64; return (ulong)(res >> 64); }
    }

    [RiscvInstruction("MULHU", InstructionSet.RV64M, RiscvEncodingType.R, 0x33, Funct3 = 3, Funct7 = 1)]
    public class MulhuInstruction : RTypeInstruction {
        public MulhuInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }
        public override void Execute(MachineState s, InstructionData d) => s.Registers.Write(d.Rd, Calc(s.Registers.Read(d.Rs1), s.Registers.Read(d.Rs2), s.Config.XLEN));
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) => op.AluResult = Calc(r1, r2, s.Config.XLEN);
        private ulong Calc(ulong v1, ulong v2, int xlen) { if (xlen == 32) return ((v1 & 0xFFFFFFFF) * (v2 & 0xFFFFFFFF)) >> 32; return (ulong)(((System.UInt128)v1 * (System.UInt128)v2) >> 64); }
    }

    [RiscvInstruction("DIV", InstructionSet.RV64M, RiscvEncodingType.R, 0x33, Funct3 = 4, Funct7 = 1)]
    public class DivInstruction : RTypeInstruction {
        public DivInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }
        public override void Execute(MachineState s, InstructionData d) => s.Registers.Write(d.Rd, Calc((long)s.Registers.Read(d.Rs1), (long)s.Registers.Read(d.Rs2)));
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) => op.AluResult = Calc((long)r1, (long)r2);
        private ulong Calc(long v1, long v2) { if (v2 == 0) return ulong.MaxValue; if (v1 == long.MinValue && v2 == -1) return unchecked((ulong)long.MinValue); return (ulong)(v1 / v2); }
    }

    [RiscvInstruction("DIVU", InstructionSet.RV64M, RiscvEncodingType.R, 0x33, Funct3 = 5, Funct7 = 1)]
    public class DivuInstruction : RTypeInstruction {
        public DivuInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }
        public override void Execute(MachineState s, InstructionData d) => s.Registers.Write(d.Rd, Calc(s.Registers.Read(d.Rs1), s.Registers.Read(d.Rs2), s.Config.XLEN));
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) => op.AluResult = Calc(r1, r2, s.Config.XLEN);
        private ulong Calc(ulong v1, ulong v2, int xlen) { if (v2 == 0) return ulong.MaxValue; ulong r = v1 / v2; return xlen == 32 ? (ulong)(uint)r : r; }
    }

    [RiscvInstruction("REM", InstructionSet.RV64M, RiscvEncodingType.R, 0x33, Funct3 = 6, Funct7 = 1)]
    public class RemInstruction : RTypeInstruction {
        public RemInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }
        public override void Execute(MachineState s, InstructionData d) => s.Registers.Write(d.Rd, Calc((long)s.Registers.Read(d.Rs1), (long)s.Registers.Read(d.Rs2)));
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) => op.AluResult = Calc((long)r1, (long)r2);
        private ulong Calc(long v1, long v2) { if (v2 == 0) return (ulong)v1; if (v1 == long.MinValue && v2 == -1) return 0; return (ulong)(v1 % v2); }
    }

    [RiscvInstruction("REMU", InstructionSet.RV64M, RiscvEncodingType.R, 0x33, Funct3 = 7, Funct7 = 1)]
    public class RemuInstruction : RTypeInstruction {
        public RemuInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }
        public override void Execute(MachineState s, InstructionData d) { ulong v2 = s.Registers.Read(d.Rs2); s.Registers.Write(d.Rd, v2 == 0 ? s.Registers.Read(d.Rs1) : s.Registers.Read(d.Rs1) % v2); }
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) => op.AluResult = r2 == 0 ? r1 : r1 % r2;
    }

    [RiscvInstruction("MULW", InstructionSet.RV64M, RiscvEncodingType.R, 0x3B, Funct3 = 0, Funct7 = 1)]
    public class MulwInstruction : RTypeInstruction {
        public MulwInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }
        public override void Execute(MachineState s, InstructionData d) => s.Registers.Write(d.Rd, (ulong)((long)(int)s.Registers.Read(d.Rs1) * (long)(int)s.Registers.Read(d.Rs2)));
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) => op.AluResult = (ulong)((long)(int)r1 * (long)(int)r2);
    }

    [RiscvInstruction("DIVW", InstructionSet.RV64M, RiscvEncodingType.R, 0x3B, Funct3 = 4, Funct7 = 1)]
    public class DivwInstruction : RTypeInstruction {
        public DivwInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }
        public override void Execute(MachineState s, InstructionData d) => s.Registers.Write(d.Rd, Calc((int)s.Registers.Read(d.Rs1), (int)s.Registers.Read(d.Rs2)));
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) => op.AluResult = Calc((int)r1, (int)r2);
        private ulong Calc(int v1, int v2) { if (v2 == 0) return ulong.MaxValue; if (v1 == int.MinValue && v2 == -1) return unchecked((ulong)(long)int.MinValue); return (ulong)(long)(v1 / v2); }
    }

    [RiscvInstruction("DIVUW", InstructionSet.RV64M, RiscvEncodingType.R, 0x3B, Funct3 = 5, Funct7 = 1)]
    public class DivuwInstruction : RTypeInstruction {
        public DivuwInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }
        public override void Execute(MachineState s, InstructionData d) { uint v2 = (uint)s.Registers.Read(d.Rs2); s.Registers.Write(d.Rd, v2 == 0 ? ulong.MaxValue : (ulong)(long)(int)((uint)s.Registers.Read(d.Rs1) / v2)); }
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) { uint v2 = (uint)r2; op.AluResult = v2 == 0 ? ulong.MaxValue : (ulong)(long)(int)((uint)r1 / v2); }
    }

    [RiscvInstruction("REMW", InstructionSet.RV64M, RiscvEncodingType.R, 0x3B, Funct3 = 6, Funct7 = 1)]
    public class RemwInstruction : RTypeInstruction {
        public RemwInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }
        public override void Execute(MachineState s, InstructionData d) => s.Registers.Write(d.Rd, Calc((int)s.Registers.Read(d.Rs1), (int)s.Registers.Read(d.Rs2)));
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) => op.AluResult = Calc((int)r1, (int)r2);
        private ulong Calc(int v1, int v2) { if (v2 == 0) return (ulong)(long)v1; if (v1 == int.MinValue && v2 == -1) return 0; return (ulong)(long)(v1 % v2); }
    }

    [RiscvInstruction("REMUW", InstructionSet.RV64M, RiscvEncodingType.R, 0x3B, Funct3 = 7, Funct7 = 1)]
    public class RemuwInstruction : RTypeInstruction {
        public RemuwInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }
        public override void Execute(MachineState s, InstructionData d) { uint v2 = (uint)s.Registers.Read(d.Rs2); s.Registers.Write(d.Rd, v2 == 0 ? (ulong)(long)(int)(uint)s.Registers.Read(d.Rs1) : (ulong)(long)(int)((uint)s.Registers.Read(d.Rs1) % v2)); }
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) { uint v2 = (uint)r2; op.AluResult = v2 == 0 ? (ulong)(long)(int)(uint)r1 : (ulong)(long)(int)((uint)r1 % v2); }
    }
}
