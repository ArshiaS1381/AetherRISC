using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.Pipeline;

namespace AetherRISC.Core.Architecture.Hardware.ISA.Extensions.B.Zba
{
    [RiscvInstruction("ADD.UW", InstructionSet.Zba, RiscvEncodingType.R, 0x3B, Funct3 = 0, Funct7 = 0x04)]
    public class AddUwInstruction : RTypeInstruction {
        public AddUwInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }
        public override void Execute(MachineState s, InstructionData d) => s.Registers.Write(d.Rd, s.Registers.Read(d.Rs1) + (s.Registers.Read(d.Rs2) & 0xFFFFFFFFul));
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) => op.AluResult = r1 + (r2 & 0xFFFFFFFFul);
    }

    [RiscvInstruction("SH1ADD", InstructionSet.Zba, RiscvEncodingType.R, 0x33, Funct3 = 2, Funct7 = 0x10)]
    public class Sh1addInstruction : RTypeInstruction {
        public Sh1addInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }
        public override void Execute(MachineState s, InstructionData d) => s.Registers.Write(d.Rd, (s.Registers.Read(d.Rs1) << 1) + s.Registers.Read(d.Rs2));
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) => op.AluResult = (r1 << 1) + r2;
    }

    [RiscvInstruction("SH1ADD.UW", InstructionSet.Zba, RiscvEncodingType.R, 0x3B, Funct3 = 2, Funct7 = 0x10)]
    public class Sh1addUwInstruction : RTypeInstruction {
        public Sh1addUwInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }
        public override void Execute(MachineState s, InstructionData d) => s.Registers.Write(d.Rd, ((s.Registers.Read(d.Rs1) & 0xFFFFFFFFul) << 1) + s.Registers.Read(d.Rs2));
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) => op.AluResult = ((r1 & 0xFFFFFFFFul) << 1) + r2;
    }

    [RiscvInstruction("SH2ADD", InstructionSet.Zba, RiscvEncodingType.R, 0x33, Funct3 = 4, Funct7 = 0x10)]
    public class Sh2addInstruction : RTypeInstruction {
        public Sh2addInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }
        public override void Execute(MachineState s, InstructionData d) => s.Registers.Write(d.Rd, (s.Registers.Read(d.Rs1) << 2) + s.Registers.Read(d.Rs2));
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) => op.AluResult = (r1 << 2) + r2;
    }

    [RiscvInstruction("SH2ADD.UW", InstructionSet.Zba, RiscvEncodingType.R, 0x3B, Funct3 = 4, Funct7 = 0x10)]
    public class Sh2addUwInstruction : RTypeInstruction {
        public Sh2addUwInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }
        public override void Execute(MachineState s, InstructionData d) => s.Registers.Write(d.Rd, ((s.Registers.Read(d.Rs1) & 0xFFFFFFFFul) << 2) + s.Registers.Read(d.Rs2));
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) => op.AluResult = ((r1 & 0xFFFFFFFFul) << 2) + r2;
    }

    [RiscvInstruction("SH3ADD", InstructionSet.Zba, RiscvEncodingType.R, 0x33, Funct3 = 6, Funct7 = 0x10)]
    public class Sh3addInstruction : RTypeInstruction {
        public Sh3addInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }
        public override void Execute(MachineState s, InstructionData d) => s.Registers.Write(d.Rd, (s.Registers.Read(d.Rs1) << 3) + s.Registers.Read(d.Rs2));
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) => op.AluResult = (r1 << 3) + r2;
    }

    [RiscvInstruction("SH3ADD.UW", InstructionSet.Zba, RiscvEncodingType.R, 0x3B, Funct3 = 6, Funct7 = 0x10)]
    public class Sh3addUwInstruction : RTypeInstruction {
        public Sh3addUwInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }
        public override void Execute(MachineState s, InstructionData d) => s.Registers.Write(d.Rd, ((s.Registers.Read(d.Rs1) & 0xFFFFFFFFul) << 3) + s.Registers.Read(d.Rs2));
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) => op.AluResult = ((r1 & 0xFFFFFFFFul) << 3) + r2;
    }

    [RiscvInstruction("SLLI.UW", InstructionSet.Zba, RiscvEncodingType.ShiftImm, 0x1B, Funct3 = 1, Funct6 = 0x02)]
    public class SlliUwInstruction : ITypeInstruction {
        public SlliUwInstruction(int rd, int rs1, int shamt) : base(rd, rs1, shamt) { }
        public override void Execute(MachineState s, InstructionData d) => s.Registers.Write(d.Rd, (s.Registers.Read(d.Rs1) & 0xFFFFFFFFul) << (int)(d.Immediate & 0x3F));
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) => op.AluResult = (r1 & 0xFFFFFFFFul) << (op.Immediate & 0x3F);
    }
}
