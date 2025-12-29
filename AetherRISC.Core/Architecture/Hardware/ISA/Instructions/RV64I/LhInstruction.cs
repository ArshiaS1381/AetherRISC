using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.Pipeline;

namespace AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64I;

[RiscvInstruction("LH", InstructionSet.RV64I, RiscvEncodingType.I, 0x03, Funct3 = 1,
    Name = "Load Half-word",
    Description = "Loads a 16-bit value from memory, sign-extends it to XLEN, and stores it in rd.",
    Usage = "lh rd, offset(rs1)")]
public class LhInstruction : ITypeInstruction
{
    public override bool IsLoad => true;

    public LhInstruction(int rd, int rs1, int imm) : base(rd, rs1, imm) { }

    public override void Execute(MachineState s, InstructionData d)
    {
        uint addr = (uint)((long)s.Registers.Read(d.Rs1) + (long)(int)d.Immediate);
        ushort val = s.Memory!.ReadHalf(addr);
        s.Registers.Write(d.Rd, (ulong)(short)val);
    }

    public override void Compute(MachineState state, ulong rs1Val, ulong rs2Val, PipelineBuffers buffers)
    {
        buffers.ExecuteMemory.AluResult = (ulong)((long)rs1Val + (long)buffers.DecodeExecute.Immediate);
    }
}
