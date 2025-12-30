using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.Pipeline;

namespace AetherRISC.Core.Architecture.Hardware.ISA.Extensions.B.Zbs
{
    [RiscvInstruction("BCLR", InstructionSet.Zbs, RiscvEncodingType.R, 0x33, Funct3 = 1, Funct7 = 0x24)]
    public class BclrInstruction : RTypeInstruction {
        public BclrInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }
        public override void Execute(MachineState s, InstructionData d) => s.Registers.Write(d.Rd, s.Registers.Read(d.Rs1) & ~(1UL << (int)(s.Registers.Read(d.Rs2) & (s.Config.XLEN == 32 ? 31U : 63U))));
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) => op.AluResult = r1 & ~(1UL << (int)(r2 & (s.Config.XLEN == 32 ? 31U : 63U)));
    }

    [RiscvInstruction("BCLRI", InstructionSet.Zbs, RiscvEncodingType.ShiftImm, 0x13, Funct3 = 1, Funct6 = 0x12)]
    public class BclriInstruction : ITypeInstruction {
        public BclriInstruction(int rd, int rs1, int imm) : base(rd, rs1, imm) { }
        public override void Execute(MachineState s, InstructionData d) => s.Registers.Write(d.Rd, s.Registers.Read(d.Rs1) & ~(1UL << (d.Imm & (s.Config.XLEN == 32 ? 31 : 63))));
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) => op.AluResult = r1 & ~(1UL << (op.Immediate & (s.Config.XLEN == 32 ? 31 : 63)));
    }

    [RiscvInstruction("BEXT", InstructionSet.Zbs, RiscvEncodingType.R, 0x33, Funct3 = 5, Funct7 = 0x24)]
    public class BextInstruction : RTypeInstruction {
        public BextInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }
        public override void Execute(MachineState s, InstructionData d) => s.Registers.Write(d.Rd, (s.Registers.Read(d.Rs1) >> (int)(s.Registers.Read(d.Rs2) & (s.Config.XLEN == 32 ? 31U : 63U))) & 1UL);
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) => op.AluResult = (r1 >> (int)(r2 & (s.Config.XLEN == 32 ? 31U : 63U))) & 1UL;
    }

    [RiscvInstruction("BEXTI", InstructionSet.Zbs, RiscvEncodingType.ShiftImm, 0x13, Funct3 = 5, Funct6 = 0x12)]
    public class BextiInstruction : ITypeInstruction {
        public BextiInstruction(int rd, int rs1, int imm) : base(rd, rs1, imm) { }
        public override void Execute(MachineState s, InstructionData d) => s.Registers.Write(d.Rd, (s.Registers.Read(d.Rs1) >> (d.Imm & (s.Config.XLEN == 32 ? 31 : 63))) & 1UL);
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) => op.AluResult = (r1 >> (op.Immediate & (s.Config.XLEN == 32 ? 31 : 63))) & 1UL;
    }

    [RiscvInstruction("BINV", InstructionSet.Zbs, RiscvEncodingType.R, 0x33, Funct3 = 1, Funct7 = 0x34)]
    public class BinvInstruction : RTypeInstruction {
        public BinvInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }
        public override void Execute(MachineState s, InstructionData d) => s.Registers.Write(d.Rd, s.Registers.Read(d.Rs1) ^ (1UL << (int)(s.Registers.Read(d.Rs2) & (s.Config.XLEN == 32 ? 31U : 63U))));
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) => op.AluResult = r1 ^ (1UL << (int)(r2 & (s.Config.XLEN == 32 ? 31U : 63U)));
    }

    [RiscvInstruction("BINVI", InstructionSet.Zbs, RiscvEncodingType.ShiftImm, 0x13, Funct3 = 1, Funct6 = 0x1A)]
    public class BinviInstruction : ITypeInstruction {
        public BinviInstruction(int rd, int rs1, int imm) : base(rd, rs1, imm) { }
        public override void Execute(MachineState s, InstructionData d) => s.Registers.Write(d.Rd, s.Registers.Read(d.Rs1) ^ (1UL << (d.Imm & (s.Config.XLEN == 32 ? 31 : 63))));
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) => op.AluResult = r1 ^ (1UL << (op.Immediate & (s.Config.XLEN == 32 ? 31 : 63)));
    }

    [RiscvInstruction("BSET", InstructionSet.Zbs, RiscvEncodingType.R, 0x33, Funct3 = 1, Funct7 = 0x14)]
    public class BsetInstruction : RTypeInstruction {
        public BsetInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }
        public override void Execute(MachineState s, InstructionData d) => s.Registers.Write(d.Rd, s.Registers.Read(d.Rs1) | (1UL << (int)(s.Registers.Read(d.Rs2) & (s.Config.XLEN == 32 ? 31U : 63U))));
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) => op.AluResult = r1 | (1UL << (int)(r2 & (s.Config.XLEN == 32 ? 31U : 63U)));
    }

    [RiscvInstruction("BSETI", InstructionSet.Zbs, RiscvEncodingType.ShiftImm, 0x13, Funct3 = 1, Funct6 = 0x0A)]
    public class BsetiInstruction : ITypeInstruction {
        public BsetiInstruction(int rd, int rs1, int imm) : base(rd, rs1, imm) { }
        public override void Execute(MachineState s, InstructionData d) => s.Registers.Write(d.Rd, s.Registers.Read(d.Rs1) | (1UL << (d.Imm & (s.Config.XLEN == 32 ? 31 : 63))));
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) => op.AluResult = r1 | (1UL << (op.Immediate & (s.Config.XLEN == 32 ? 31 : 63)));
    }
}
