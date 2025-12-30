using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.Pipeline;

namespace AetherRISC.Core.Architecture.Hardware.ISA.Extensions.Zicsr
{
    [RiscvInstruction("CSRRW", InstructionSet.Zicsr, RiscvEncodingType.I, 0x73, Funct3 = 1)]
    public class CsrrwInstruction : ITypeInstruction {
        public CsrrwInstruction(int rd, int rs1, int imm) : base(rd, rs1, imm) { }
        public override void Execute(MachineState s, InstructionData d) { uint addr = (uint)d.Imm & 0xFFF; ulong old = s.Csr.Read(addr); s.Csr.Write(addr, s.Registers.Read(d.Rs1)); s.Registers.Write(d.Rd, old); }
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) { uint addr = (uint)op.Immediate & 0xFFF; ulong old = s.Csr.Read(addr); s.Csr.Write(addr, r1); op.AluResult = old; }
    }

    [RiscvInstruction("CSRRS", InstructionSet.Zicsr, RiscvEncodingType.I, 0x73, Funct3 = 2)]
    public class CsrrsInstruction : ITypeInstruction {
        public CsrrsInstruction(int rd, int rs1, int imm) : base(rd, rs1, imm) { }
        public override void Execute(MachineState s, InstructionData d) { uint addr = (uint)d.Imm & 0xFFF; ulong old = s.Csr.Read(addr); if (d.Rs1 != 0) s.Csr.Write(addr, old | s.Registers.Read(d.Rs1)); s.Registers.Write(d.Rd, old); }
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) { uint addr = (uint)op.Immediate & 0xFFF; ulong old = s.Csr.Read(addr); if (this.Rs1 != 0) s.Csr.Write(addr, old | r1); op.AluResult = old; }
    }

    [RiscvInstruction("CSRRC", InstructionSet.Zicsr, RiscvEncodingType.I, 0x73, Funct3 = 3)]
    public class CsrrcInstruction : ITypeInstruction {
        public CsrrcInstruction(int rd, int rs1, int imm) : base(rd, rs1, imm) { }
        public override void Execute(MachineState s, InstructionData d) { uint addr = (uint)d.Imm & 0xFFF; ulong old = s.Csr.Read(addr); if (d.Rs1 != 0) s.Csr.Write(addr, old & ~s.Registers.Read(d.Rs1)); s.Registers.Write(d.Rd, old); }
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) { uint addr = (uint)op.Immediate & 0xFFF; ulong old = s.Csr.Read(addr); if (this.Rs1 != 0) s.Csr.Write(addr, old & ~r1); op.AluResult = old; }
    }

    [RiscvInstruction("CSRRWI", InstructionSet.Zicsr, RiscvEncodingType.I, 0x73, Funct3 = 5)]
    public class CsrrwiInstruction : ITypeInstruction {
        public CsrrwiInstruction(int rd, int rs1, int imm) : base(rd, rs1, imm) { }
        public override void Execute(MachineState s, InstructionData d) { uint addr = (uint)d.Imm & 0xFFF; ulong old = s.Csr.Read(addr); s.Csr.Write(addr, (ulong)(d.Rs1 & 0x1F)); s.Registers.Write(d.Rd, old); }
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) { uint addr = (uint)op.Immediate & 0xFFF; ulong old = s.Csr.Read(addr); s.Csr.Write(addr, (ulong)(this.Rs1 & 0x1F)); op.AluResult = old; }
    }

    [RiscvInstruction("CSRRSI", InstructionSet.Zicsr, RiscvEncodingType.I, 0x73, Funct3 = 6)]
    public class CsrrsiInstruction : ITypeInstruction {
        public CsrrsiInstruction(int rd, int rs1, int imm) : base(rd, rs1, imm) { }
        public override void Execute(MachineState s, InstructionData d) { uint addr = (uint)d.Imm & 0xFFF; ulong old = s.Csr.Read(addr); ulong uimm = (ulong)(d.Rs1 & 0x1F); if (uimm != 0) s.Csr.Write(addr, old | uimm); s.Registers.Write(d.Rd, old); }
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) { uint addr = (uint)op.Immediate & 0xFFF; ulong old = s.Csr.Read(addr); ulong uimm = (ulong)(this.Rs1 & 0x1F); if (uimm != 0) s.Csr.Write(addr, old | uimm); op.AluResult = old; }
    }

    [RiscvInstruction("CSRRCI", InstructionSet.Zicsr, RiscvEncodingType.I, 0x73, Funct3 = 7)]
    public class CsrrciInstruction : ITypeInstruction {
        public CsrrciInstruction(int rd, int rs1, int imm) : base(rd, rs1, imm) { }
        public override void Execute(MachineState s, InstructionData d) { uint addr = (uint)d.Imm & 0xFFF; ulong old = s.Csr.Read(addr); ulong uimm = (ulong)(d.Rs1 & 0x1F); if (uimm != 0) s.Csr.Write(addr, old & ~uimm); s.Registers.Write(d.Rd, old); }
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) { uint addr = (uint)op.Immediate & 0xFFF; ulong old = s.Csr.Read(addr); ulong uimm = (ulong)(this.Rs1 & 0x1F); if (uimm != 0) s.Csr.Write(addr, old & ~uimm); op.AluResult = old; }
    }
}
