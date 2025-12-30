using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.Pipeline;

namespace AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RvSystem
{
    [RiscvInstruction("ECALL", InstructionSet.RV64I, RiscvEncodingType.I, 0x73, Funct3 = 0)]
    public class EcallInstruction : ITypeInstruction {
        public EcallInstruction(int rd, int rs1, int imm) : base(rd, rs1, 0) { }
        public override void Execute(MachineState s, InstructionData d) => s.Host?.HandleEcall(s);
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) => op.AluResult = 0;
    }

    [RiscvInstruction("EBREAK", InstructionSet.RV64I, RiscvEncodingType.I, 0x73, Funct3 = 0)]
    public class EbreakInstruction : ITypeInstruction {
        public EbreakInstruction(int rd, int rs1, int imm) : base(rd, rs1, 1) { }
        public override void Execute(MachineState s, InstructionData d) { s.Halted = true; s.Host?.HandleBreak(s); }
        public override void Compute(MachineState s, ulong r1, ulong r2, PipelineMicroOp op) => op.AluResult = 0;
    }
}
