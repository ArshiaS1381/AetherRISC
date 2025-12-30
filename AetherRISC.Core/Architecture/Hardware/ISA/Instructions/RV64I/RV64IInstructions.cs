using System;
using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.Pipeline;

namespace AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64I
{
    // ==========================================
    // 1. INTEGER COMPUTATIONAL (IMM)
    // ==========================================

    [RiscvInstruction("ADDI", InstructionSet.RV64I, RiscvEncodingType.I, 0x13, Funct3 = 0)]
    public class AddiInstruction : ITypeInstruction {
        public AddiInstruction(int rd, int rs1, int imm) : base(rd, rs1, imm) {}
        public override void Execute(MachineState s, InstructionData d) => s.Registers.Write(d.Rd, s.Registers.Read(d.Rs1) + (ulong)(long)d.Immediate);
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) => op.AluResult = r1 + (ulong)(long)op.Immediate;
    }

    [RiscvInstruction("SLTI", InstructionSet.RV64I, RiscvEncodingType.I, 0x13, Funct3 = 2)]
    public class SltiInstruction : ITypeInstruction {
        public SltiInstruction(int rd, int rs1, int imm) : base(rd, rs1, imm) {}
        public override void Execute(MachineState s, InstructionData d) => s.Registers.Write(d.Rd, (long)s.Registers.Read(d.Rs1) < (long)d.Immediate ? 1UL : 0UL);
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) => op.AluResult = (long)r1 < (long)op.Immediate ? 1UL : 0UL;
    }

    [RiscvInstruction("SLTIU", InstructionSet.RV64I, RiscvEncodingType.I, 0x13, Funct3 = 3)]
    public class SltiuInstruction : ITypeInstruction {
        public SltiuInstruction(int rd, int rs1, int imm) : base(rd, rs1, imm) {}
        public override void Execute(MachineState s, InstructionData d) => s.Registers.Write(d.Rd, s.Registers.Read(d.Rs1) < (ulong)(long)d.Immediate ? 1UL : 0UL);
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) => op.AluResult = r1 < (ulong)(long)op.Immediate ? 1UL : 0UL;
    }

    [RiscvInstruction("ANDI", InstructionSet.RV64I, RiscvEncodingType.I, 0x13, Funct3 = 7)]
    public class AndiInstruction : ITypeInstruction {
        public AndiInstruction(int rd, int rs1, int imm) : base(rd, rs1, imm) {}
        public override void Execute(MachineState s, InstructionData d) => s.Registers.Write(d.Rd, s.Registers.Read(d.Rs1) & (ulong)(long)d.Immediate);
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) => op.AluResult = r1 & (ulong)(long)op.Immediate;
    }

    [RiscvInstruction("ORI", InstructionSet.RV64I, RiscvEncodingType.I, 0x13, Funct3 = 6)]
    public class OriInstruction : ITypeInstruction {
        public OriInstruction(int rd, int rs1, int imm) : base(rd, rs1, imm) {}
        public override void Execute(MachineState s, InstructionData d) => s.Registers.Write(d.Rd, s.Registers.Read(d.Rs1) | (ulong)(long)d.Immediate);
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) => op.AluResult = r1 | (ulong)(long)op.Immediate;
    }

    [RiscvInstruction("XORI", InstructionSet.RV64I, RiscvEncodingType.I, 0x13, Funct3 = 4)]
    public class XoriInstruction : ITypeInstruction {
        public XoriInstruction(int rd, int rs1, int imm) : base(rd, rs1, imm) {}
        public override void Execute(MachineState s, InstructionData d) => s.Registers.Write(d.Rd, s.Registers.Read(d.Rs1) ^ (ulong)(long)d.Immediate);
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) => op.AluResult = r1 ^ (ulong)(long)op.Immediate;
    }

    [RiscvInstruction("SLLI", InstructionSet.RV64I, RiscvEncodingType.ShiftImm, 0x13, Funct3 = 1, Funct6 = 0x00)]
    public class SlliInstruction : ITypeInstruction {
        public SlliInstruction(int rd, int rs1, int imm) : base(rd, rs1, imm) {}
        public override void Execute(MachineState s, InstructionData d) => s.Registers.Write(d.Rd, s.Registers.Read(d.Rs1) << (d.Imm & 0x3F));
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) => op.AluResult = r1 << (op.Immediate & 0x3F);
    }

    [RiscvInstruction("SRLI", InstructionSet.RV64I, RiscvEncodingType.ShiftImm, 0x13, Funct3 = 5, Funct6 = 0x00)]
    public class SrliInstruction : ITypeInstruction {
        public SrliInstruction(int rd, int rs1, int imm) : base(rd, rs1, imm) {}
        public override void Execute(MachineState s, InstructionData d) => s.Registers.Write(d.Rd, s.Registers.Read(d.Rs1) >> (d.Imm & 0x3F));
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) => op.AluResult = r1 >> (op.Immediate & 0x3F);
    }

    [RiscvInstruction("SRAI", InstructionSet.RV64I, RiscvEncodingType.ShiftImm, 0x13, Funct3 = 5, Funct6 = 0x10)]
    public class SraiInstruction : ITypeInstruction {
        public SraiInstruction(int rd, int rs1, int imm) : base(rd, rs1, imm) {}
        public override void Execute(MachineState s, InstructionData d) => s.Registers.Write(d.Rd, (ulong)((long)s.Registers.Read(d.Rs1) >> (d.Imm & 0x3F)));
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) => op.AluResult = (ulong)((long)r1 >> (op.Immediate & 0x3F));
    }

    [RiscvInstruction("LUI", InstructionSet.RV64I, RiscvEncodingType.U, 0x37)]
    public class LuiInstruction : UTypeInstruction {
        public LuiInstruction(int rd, int imm) : base(rd, imm) {}
        public override void Execute(MachineState s, InstructionData d) => s.Registers.Write(d.Rd, (ulong)(long)(int)d.Immediate);
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) => op.AluResult = (ulong)(long)op.Immediate;
    }

    [RiscvInstruction("AUIPC", InstructionSet.RV64I, RiscvEncodingType.U, 0x17)]
    public class AuipcInstruction : UTypeInstruction {
        public AuipcInstruction(int rd, int imm) : base(rd, imm) {}
        public override void Execute(MachineState s, InstructionData d) => s.Registers.Write(d.Rd, d.PC + (ulong)(long)(int)d.Immediate);
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) => op.AluResult = op.PC + (ulong)(long)op.Immediate;
    }

    // ==========================================
    // 2. INTEGER COMPUTATIONAL (REG)
    // ==========================================

    [RiscvInstruction("ADD", InstructionSet.RV64I, RiscvEncodingType.R, 0x33, Funct3 = 0, Funct7 = 0)]
    public class AddInstruction : RTypeInstruction {
        public AddInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) {}
        public override void Execute(MachineState s, InstructionData d) => s.Registers.Write(d.Rd, s.Registers.Read(d.Rs1) + s.Registers.Read(d.Rs2));
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) => op.AluResult = r1 + r2;
    }

    [RiscvInstruction("SUB", InstructionSet.RV64I, RiscvEncodingType.R, 0x33, Funct3 = 0, Funct7 = 0x20)]
    public class SubInstruction : RTypeInstruction {
        public SubInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) {}
        public override void Execute(MachineState s, InstructionData d) => s.Registers.Write(d.Rd, s.Registers.Read(d.Rs1) - s.Registers.Read(d.Rs2));
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) => op.AluResult = r1 - r2;
    }

    [RiscvInstruction("SLL", InstructionSet.RV64I, RiscvEncodingType.R, 0x33, Funct3 = 1, Funct7 = 0)]
    public class SllInstruction : RTypeInstruction {
        public SllInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) {}
        public override void Execute(MachineState s, InstructionData d) => s.Registers.Write(d.Rd, s.Registers.Read(d.Rs1) << ((int)s.Registers.Read(d.Rs2) & 0x3F));
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) => op.AluResult = r1 << ((int)r2 & 0x3F);
    }

    [RiscvInstruction("SLT", InstructionSet.RV64I, RiscvEncodingType.R, 0x33, Funct3 = 2, Funct7 = 0)]
    public class SltInstruction : RTypeInstruction {
        public SltInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) {}
        public override void Execute(MachineState s, InstructionData d) => s.Registers.Write(d.Rd, (long)s.Registers.Read(d.Rs1) < (long)s.Registers.Read(d.Rs2) ? 1UL : 0UL);
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) => op.AluResult = (long)r1 < (long)r2 ? 1UL : 0UL;
    }

    [RiscvInstruction("SLTU", InstructionSet.RV64I, RiscvEncodingType.R, 0x33, Funct3 = 3, Funct7 = 0)]
    public class SltuInstruction : RTypeInstruction {
        public SltuInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) {}
        public override void Execute(MachineState s, InstructionData d) => s.Registers.Write(d.Rd, s.Registers.Read(d.Rs1) < s.Registers.Read(d.Rs2) ? 1UL : 0UL);
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) => op.AluResult = r1 < r2 ? 1UL : 0UL;
    }

    [RiscvInstruction("XOR", InstructionSet.RV64I, RiscvEncodingType.R, 0x33, Funct3 = 4, Funct7 = 0)]
    public class XorInstruction : RTypeInstruction {
        public XorInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) {}
        public override void Execute(MachineState s, InstructionData d) => s.Registers.Write(d.Rd, s.Registers.Read(d.Rs1) ^ s.Registers.Read(d.Rs2));
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) => op.AluResult = r1 ^ r2;
    }

    [RiscvInstruction("SRL", InstructionSet.RV64I, RiscvEncodingType.R, 0x33, Funct3 = 5, Funct7 = 0)]
    public class SrlInstruction : RTypeInstruction {
        public SrlInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) {}
        public override void Execute(MachineState s, InstructionData d) => s.Registers.Write(d.Rd, s.Registers.Read(d.Rs1) >> ((int)s.Registers.Read(d.Rs2) & 0x3F));
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) => op.AluResult = r1 >> ((int)r2 & 0x3F);
    }

    [RiscvInstruction("SRA", InstructionSet.RV64I, RiscvEncodingType.R, 0x33, Funct3 = 5, Funct7 = 0x20)]
    public class SraInstruction : RTypeInstruction {
        public SraInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) {}
        public override void Execute(MachineState s, InstructionData d) => s.Registers.Write(d.Rd, (ulong)((long)s.Registers.Read(d.Rs1) >> ((int)s.Registers.Read(d.Rs2) & 0x3F)));
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) => op.AluResult = (ulong)((long)r1 >> ((int)r2 & 0x3F));
    }

    [RiscvInstruction("OR", InstructionSet.RV64I, RiscvEncodingType.R, 0x33, Funct3 = 6, Funct7 = 0)]
    public class OrInstruction : RTypeInstruction {
        public OrInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) {}
        public override void Execute(MachineState s, InstructionData d) => s.Registers.Write(d.Rd, s.Registers.Read(d.Rs1) | s.Registers.Read(d.Rs2));
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) => op.AluResult = r1 | r2;
    }

    [RiscvInstruction("AND", InstructionSet.RV64I, RiscvEncodingType.R, 0x33, Funct3 = 7, Funct7 = 0)]
    public class AndInstruction : RTypeInstruction {
        public AndInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) {}
        public override void Execute(MachineState s, InstructionData d) => s.Registers.Write(d.Rd, s.Registers.Read(d.Rs1) & s.Registers.Read(d.Rs2));
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) => op.AluResult = r1 & r2;
    }

    // ==========================================
    // 3. CONTROL TRANSFER
    // ==========================================

    [RiscvInstruction("JAL", InstructionSet.RV64I, RiscvEncodingType.J, 0x6F)]
    public class JalInstruction : JTypeInstruction {
        public override bool IsJump => true;
        public JalInstruction(int rd, int imm) : base(rd, imm) {}
        public override void Execute(MachineState s, InstructionData d) {
            s.Registers.Write(d.Rd, d.PC + 4);
            s.ProgramCounter = d.PC + (ulong)(long)d.Immediate;
        }
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) {
            op.AluResult = op.PC + 4;
            op.BranchTaken = true;
            op.ActualTarget = op.PC + (ulong)(long)op.Immediate;
        }
    }

    [RiscvInstruction("JALR", InstructionSet.RV64I, RiscvEncodingType.I, 0x67, Funct3 = 0)]
    public class JalrInstruction : ITypeInstruction {
        public override bool IsJump => true;
        public JalrInstruction(int rd, int rs1, int imm) : base(rd, rs1, imm) {}
        public override void Execute(MachineState s, InstructionData d) {
            ulong target = (s.Registers.Read(d.Rs1) + (ulong)(long)d.Immediate) & ~1UL;
            s.Registers.Write(d.Rd, d.PC + 4);
            s.ProgramCounter = target;
        }
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) {
            op.AluResult = op.PC + 4;
            op.BranchTaken = true;
            op.ActualTarget = (r1 + (ulong)(long)op.Immediate) & ~1UL;
        }
    }

    [RiscvInstruction("BEQ", InstructionSet.RV64I, RiscvEncodingType.B, 0x63, Funct3 = 0)]
    public class BeqInstruction : BTypeInstruction {
        public override bool IsBranch => true;
        public BeqInstruction(int rs1, int rs2, int imm) : base(rs1, rs2, imm) {}
        public override void Execute(MachineState s, InstructionData d) {
            if (s.Registers.Read(d.Rs1) == s.Registers.Read(d.Rs2))
                s.ProgramCounter = d.PC + (ulong)(long)d.Immediate;
        }
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) {
            if (r1 == r2) {
                op.BranchTaken = true;
                op.ActualTarget = op.PC + (ulong)(long)op.Immediate;
            }
        }
    }

    [RiscvInstruction("BNE", InstructionSet.RV64I, RiscvEncodingType.B, 0x63, Funct3 = 1)]
    public class BneInstruction : BTypeInstruction {
        public override bool IsBranch => true;
        public BneInstruction(int rs1, int rs2, int imm) : base(rs1, rs2, imm) {}
        public override void Execute(MachineState s, InstructionData d) {
            if (s.Registers.Read(d.Rs1) != s.Registers.Read(d.Rs2))
                s.ProgramCounter = d.PC + (ulong)(long)d.Immediate;
        }
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) {
            if (r1 != r2) {
                op.BranchTaken = true;
                op.ActualTarget = op.PC + (ulong)(long)op.Immediate;
            }
        }
    }

    [RiscvInstruction("BLT", InstructionSet.RV64I, RiscvEncodingType.B, 0x63, Funct3 = 4)]
    public class BltInstruction : BTypeInstruction {
        public override bool IsBranch => true;
        public BltInstruction(int rs1, int rs2, int imm) : base(rs1, rs2, imm) {}
        public override void Execute(MachineState s, InstructionData d) {
            if ((long)s.Registers.Read(d.Rs1) < (long)s.Registers.Read(d.Rs2))
                s.ProgramCounter = d.PC + (ulong)(long)d.Immediate;
        }
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) {
            if ((long)r1 < (long)r2) {
                op.BranchTaken = true;
                op.ActualTarget = op.PC + (ulong)(long)op.Immediate;
            }
        }
    }

    [RiscvInstruction("BGE", InstructionSet.RV64I, RiscvEncodingType.B, 0x63, Funct3 = 5)]
    public class BgeInstruction : BTypeInstruction {
        public override bool IsBranch => true;
        public BgeInstruction(int rs1, int rs2, int imm) : base(rs1, rs2, imm) {}
        public override void Execute(MachineState s, InstructionData d) {
            if ((long)s.Registers.Read(d.Rs1) >= (long)s.Registers.Read(d.Rs2))
                s.ProgramCounter = d.PC + (ulong)(long)d.Immediate;
        }
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) {
            if ((long)r1 >= (long)r2) {
                op.BranchTaken = true;
                op.ActualTarget = op.PC + (ulong)(long)op.Immediate;
            }
        }
    }

    [RiscvInstruction("BLTU", InstructionSet.RV64I, RiscvEncodingType.B, 0x63, Funct3 = 6)]
    public class BltuInstruction : BTypeInstruction {
        public override bool IsBranch => true;
        public BltuInstruction(int rs1, int rs2, int imm) : base(rs1, rs2, imm) {}
        public override void Execute(MachineState s, InstructionData d) {
            if (s.Registers.Read(d.Rs1) < s.Registers.Read(d.Rs2))
                s.ProgramCounter = d.PC + (ulong)(long)d.Immediate;
        }
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) {
            if (r1 < r2) {
                op.BranchTaken = true;
                op.ActualTarget = op.PC + (ulong)(long)op.Immediate;
            }
        }
    }

    [RiscvInstruction("BGEU", InstructionSet.RV64I, RiscvEncodingType.B, 0x63, Funct3 = 7)]
    public class BgeuInstruction : BTypeInstruction {
        public override bool IsBranch => true;
        public BgeuInstruction(int rs1, int rs2, int imm) : base(rs1, rs2, imm) {}
        public override void Execute(MachineState s, InstructionData d) {
            if (s.Registers.Read(d.Rs1) >= s.Registers.Read(d.Rs2))
                s.ProgramCounter = d.PC + (ulong)(long)d.Immediate;
        }
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) {
            if (r1 >= r2) {
                op.BranchTaken = true;
                op.ActualTarget = op.PC + (ulong)(long)op.Immediate;
            }
        }
    }

    // ==========================================
    // 4. LOAD / STORE
    // ==========================================

    [RiscvInstruction("LB", InstructionSet.RV64I, RiscvEncodingType.I, 0x03, Funct3 = 0)]
    public class LbInstruction : ITypeInstruction {
        public override bool IsLoad => true;
        public LbInstruction(int rd, int rs1, int imm) : base(rd, rs1, imm) {}
        public override void Execute(MachineState s, InstructionData d) => s.Registers.Write(d.Rd, (ulong)(long)(sbyte)s.Memory!.ReadByte((uint)((long)s.Registers.Read(d.Rs1) + (long)d.Immediate)));
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) => op.AluResult = (ulong)((long)r1 + (long)op.Immediate);
    }

    [RiscvInstruction("LH", InstructionSet.RV64I, RiscvEncodingType.I, 0x03, Funct3 = 1)]
    public class LhInstruction : ITypeInstruction {
        public override bool IsLoad => true;
        public LhInstruction(int rd, int rs1, int imm) : base(rd, rs1, imm) {}
        public override void Execute(MachineState s, InstructionData d) => s.Registers.Write(d.Rd, (ulong)(long)(short)s.Memory!.ReadHalf((uint)((long)s.Registers.Read(d.Rs1) + (long)d.Immediate)));
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) => op.AluResult = (ulong)((long)r1 + (long)op.Immediate);
    }

    [RiscvInstruction("LW", InstructionSet.RV64I, RiscvEncodingType.I, 0x03, Funct3 = 2)]
    public class LwInstruction : ITypeInstruction {
        public override bool IsLoad => true;
        public LwInstruction(int rd, int rs1, int imm) : base(rd, rs1, imm) {}
        public override void Execute(MachineState s, InstructionData d) => s.Registers.Write(d.Rd, (ulong)(long)(int)s.Memory!.ReadWord((uint)((long)s.Registers.Read(d.Rs1) + (long)d.Immediate)));
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) => op.AluResult = (ulong)((long)r1 + (long)op.Immediate);
    }

    [RiscvInstruction("LBU", InstructionSet.RV64I, RiscvEncodingType.I, 0x03, Funct3 = 4)]
    public class LbuInstruction : ITypeInstruction {
        public override bool IsLoad => true;
        public LbuInstruction(int rd, int rs1, int imm) : base(rd, rs1, imm) {}
        public override void Execute(MachineState s, InstructionData d) => s.Registers.Write(d.Rd, (ulong)s.Memory!.ReadByte((uint)((long)s.Registers.Read(d.Rs1) + (long)d.Immediate)));
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) => op.AluResult = (ulong)((long)r1 + (long)op.Immediate);
    }

    [RiscvInstruction("LHU", InstructionSet.RV64I, RiscvEncodingType.I, 0x03, Funct3 = 5)]
    public class LhuInstruction : ITypeInstruction {
        public override bool IsLoad => true;
        public LhuInstruction(int rd, int rs1, int imm) : base(rd, rs1, imm) {}
        public override void Execute(MachineState s, InstructionData d) => s.Registers.Write(d.Rd, (ulong)s.Memory!.ReadHalf((uint)((long)s.Registers.Read(d.Rs1) + (long)d.Immediate)));
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) => op.AluResult = (ulong)((long)r1 + (long)op.Immediate);
    }

    [RiscvInstruction("SB", InstructionSet.RV64I, RiscvEncodingType.S, 0x23, Funct3 = 0)]
    public class SbInstruction : STypeInstruction {
        public override bool IsStore => true;
        public SbInstruction(int rs1, int rs2, int imm) : base(rs1, rs2, imm) {}
        public override void Execute(MachineState s, InstructionData d) => s.Memory!.WriteByte((uint)((long)s.Registers.Read(d.Rs1) + (long)d.Immediate), (byte)s.Registers.Read(d.Rs2));
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) {
            op.AluResult = (ulong)((long)r1 + (long)op.Immediate);
            op.StoreValue = r2 & 0xFF;
        }
    }

    [RiscvInstruction("SH", InstructionSet.RV64I, RiscvEncodingType.S, 0x23, Funct3 = 1)]
    public class ShInstruction : STypeInstruction {
        public override bool IsStore => true;
        public ShInstruction(int rs1, int rs2, int imm) : base(rs1, rs2, imm) {}
        public override void Execute(MachineState s, InstructionData d) => s.Memory!.WriteHalf((uint)((long)s.Registers.Read(d.Rs1) + (long)d.Immediate), (ushort)s.Registers.Read(d.Rs2));
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) {
            op.AluResult = (ulong)((long)r1 + (long)op.Immediate);
            op.StoreValue = r2 & 0xFFFF;
        }
    }

    [RiscvInstruction("SW", InstructionSet.RV64I, RiscvEncodingType.S, 0x23, Funct3 = 2)]
    public class SwInstruction : STypeInstruction {
        public override bool IsStore => true;
        public SwInstruction(int rs1, int rs2, int imm) : base(rs1, rs2, imm) {}
        public override void Execute(MachineState s, InstructionData d) => s.Memory!.WriteWord((uint)((long)s.Registers.Read(d.Rs1) + (long)d.Immediate), (uint)s.Registers.Read(d.Rs2));
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) {
            op.AluResult = (ulong)((long)r1 + (long)op.Immediate);
            op.StoreValue = r2 & 0xFFFFFFFF;
        }
    }

    // ==========================================
    // 5. RV64 SPECIFIC (WORD/DOUBLE)
    // ==========================================

    [RiscvInstruction("LWU", InstructionSet.RV64I, RiscvEncodingType.I, 0x03, Funct3 = 6)]
    public class LwuInstruction : ITypeInstruction {
        public override bool IsLoad => true;
        public LwuInstruction(int rd, int rs1, int imm) : base(rd, rs1, imm) {}
        public override void Execute(MachineState s, InstructionData d) => s.Registers.Write(d.Rd, (ulong)s.Memory!.ReadWord((uint)((long)s.Registers.Read(d.Rs1) + (long)d.Immediate)));
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) => op.AluResult = (ulong)((long)r1 + (long)op.Immediate);
    }

    [RiscvInstruction("LD", InstructionSet.RV64I, RiscvEncodingType.I, 0x03, Funct3 = 3)]
    public class LdInstruction : ITypeInstruction {
        public override bool IsLoad => true;
        public LdInstruction(int rd, int rs1, int imm) : base(rd, rs1, imm) {}
        public override void Execute(MachineState s, InstructionData d) => s.Registers.Write(d.Rd, s.Memory!.ReadDouble((uint)((long)s.Registers.Read(d.Rs1) + (long)d.Immediate)));
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) => op.AluResult = (ulong)((long)r1 + (long)op.Immediate);
    }

    [RiscvInstruction("SD", InstructionSet.RV64I, RiscvEncodingType.S, 0x23, Funct3 = 3)]
    public class SdInstruction : STypeInstruction {
        public override bool IsStore => true;
        public SdInstruction(int rs1, int rs2, int imm) : base(rs1, rs2, imm) {}
        public override void Execute(MachineState s, InstructionData d) => s.Memory!.WriteDouble((uint)((long)s.Registers.Read(d.Rs1) + (long)d.Immediate), s.Registers.Read(d.Rs2));
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) {
            op.AluResult = (ulong)((long)r1 + (long)op.Immediate);
            op.StoreValue = r2;
        }
    }

    [RiscvInstruction("ADDIW", InstructionSet.RV64I, RiscvEncodingType.I, 0x1B, Funct3 = 0)]
    public class AddiwInstruction : ITypeInstruction {
        public AddiwInstruction(int rd, int rs1, int imm) : base(rd, rs1, imm) {}
        public override void Execute(MachineState s, InstructionData d) => s.Registers.Write(d.Rd, (ulong)(long)(int)((long)s.Registers.Read(d.Rs1) + (long)d.Immediate));
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) => op.AluResult = (ulong)(long)(int)((long)r1 + (long)op.Immediate);
    }

    [RiscvInstruction("SLLIW", InstructionSet.RV64I, RiscvEncodingType.ShiftImm, 0x1B, Funct3 = 1, Funct6 = 0x00)]
    public class SlliwInstruction : ITypeInstruction {
        public SlliwInstruction(int rd, int rs1, int imm) : base(rd, rs1, imm) {}
        public override void Execute(MachineState s, InstructionData d) => s.Registers.Write(d.Rd, (ulong)(long)(int)((long)s.Registers.Read(d.Rs1) << (d.Imm & 0x1F)));
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) => op.AluResult = (ulong)(long)(int)((long)r1 << (op.Immediate & 0x1F));
    }

    [RiscvInstruction("SRLIW", InstructionSet.RV64I, RiscvEncodingType.ShiftImm, 0x1B, Funct3 = 5, Funct6 = 0x00)]
    public class SrliwInstruction : ITypeInstruction {
        public SrliwInstruction(int rd, int rs1, int imm) : base(rd, rs1, imm) {}
        public override void Execute(MachineState s, InstructionData d) => s.Registers.Write(d.Rd, (ulong)(long)(int)((uint)s.Registers.Read(d.Rs1) >> (d.Imm & 0x1F)));
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) => op.AluResult = (ulong)(long)(int)((uint)r1 >> (op.Immediate & 0x1F));
    }

    [RiscvInstruction("SRAIW", InstructionSet.RV64I, RiscvEncodingType.ShiftImm, 0x1B, Funct3 = 5, Funct6 = 0x20)]
    public class SraiwInstruction : ITypeInstruction {
        public SraiwInstruction(int rd, int rs1, int imm) : base(rd, rs1, imm) {}
        public override void Execute(MachineState s, InstructionData d) => s.Registers.Write(d.Rd, (ulong)((int)s.Registers.Read(d.Rs1) >> (d.Imm & 0x1F)));
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) => op.AluResult = (ulong)((int)r1 >> (op.Immediate & 0x1F));
    }

    [RiscvInstruction("ADDW", InstructionSet.RV64I, RiscvEncodingType.R, 0x3B, Funct3 = 0, Funct7 = 0)]
    public class AddwInstruction : RTypeInstruction {
        public AddwInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) {}
        public override void Execute(MachineState s, InstructionData d) => s.Registers.Write(d.Rd, (ulong)(long)(int)((long)s.Registers.Read(d.Rs1) + (long)s.Registers.Read(d.Rs2)));
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) => op.AluResult = (ulong)(long)(int)((long)r1 + (long)r2);
    }

    [RiscvInstruction("SUBW", InstructionSet.RV64I, RiscvEncodingType.R, 0x3B, Funct3 = 0, Funct7 = 0x20)]
    public class SubwInstruction : RTypeInstruction {
        public SubwInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) {}
        public override void Execute(MachineState s, InstructionData d) => s.Registers.Write(d.Rd, (ulong)(long)(int)((long)s.Registers.Read(d.Rs1) - (long)s.Registers.Read(d.Rs2)));
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) => op.AluResult = (ulong)(long)(int)((long)r1 - (long)r2);
    }

    [RiscvInstruction("SLLW", InstructionSet.RV64I, RiscvEncodingType.R, 0x3B, Funct3 = 1, Funct7 = 0)]
    public class SllwInstruction : RTypeInstruction {
        public SllwInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) {}
        public override void Execute(MachineState s, InstructionData d) => s.Registers.Write(d.Rd, (ulong)(long)(int)((long)s.Registers.Read(d.Rs1) << ((int)s.Registers.Read(d.Rs2) & 0x1F)));
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) => op.AluResult = (ulong)(long)(int)((long)r1 << ((int)r2 & 0x1F));
    }

    [RiscvInstruction("SRLW", InstructionSet.RV64I, RiscvEncodingType.R, 0x3B, Funct3 = 5, Funct7 = 0)]
    public class SrlwInstruction : RTypeInstruction {
        public SrlwInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) {}
        public override void Execute(MachineState s, InstructionData d) => s.Registers.Write(d.Rd, (ulong)(long)(int)((uint)s.Registers.Read(d.Rs1) >> ((int)s.Registers.Read(d.Rs2) & 0x1F)));
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) => op.AluResult = (ulong)(long)(int)((uint)r1 >> ((int)r2 & 0x1F));
    }

    [RiscvInstruction("SRAW", InstructionSet.RV64I, RiscvEncodingType.R, 0x3B, Funct3 = 5, Funct7 = 0x20)]
    public class SrawInstruction : RTypeInstruction {
        public SrawInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) {}
        public override void Execute(MachineState s, InstructionData d) => s.Registers.Write(d.Rd, (ulong)((int)s.Registers.Read(d.Rs1) >> ((int)s.Registers.Read(d.Rs2) & 0x1F)));
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) => op.AluResult = (ulong)((int)r1 >> ((int)r2 & 0x1F));
    }
}
