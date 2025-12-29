using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.Pipeline;

namespace AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64I;

[RiscvInstruction("LD", InstructionSet.RV64I, RiscvEncodingType.I, 0x03, Funct3 = 3,
    Name = "Load Double-word",
    Description = "Loads a 64-bit value from memory and stores it in rd. (RV64 Only)",
    Usage = "ld rd, offset(rs1)")]
public class LdInstruction : ITypeInstruction
{
    public override bool IsLoad => true;

    public LdInstruction(int rd, int rs1, int imm) : base(rd, rs1, imm) { }

    public override void Execute(MachineState s, InstructionData d)
    {
        uint addr = (uint)((long)s.Registers.Read(d.Rs1) + (long)(int)d.Immediate);
        ulong val = s.Memory!.ReadDouble(addr);
        s.Registers.Write(d.Rd, val);
    }

    public override void Compute(MachineState state, ulong rs1Val, ulong rs2Val, PipelineBuffers buffers)
    {
        buffers.ExecuteMemory.AluResult = (ulong)((long)rs1Val + (long)buffers.DecodeExecute.Immediate);
    }
}
