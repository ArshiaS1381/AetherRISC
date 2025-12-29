using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.Pipeline;

namespace AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64I;

[RiscvInstruction("LW", InstructionSet.RV64I, RiscvEncodingType.I, 0x03, Funct3 = 2,
    Name = "Load Word", 
    Description = "Loads a 32-bit value from memory at rs1 + offset, sign-extends it to 64 bits, and stores it in rd.", 
    Usage = "lw rd, offset(rs1)")]
public class LwInstruction : ITypeInstruction
{
    public override bool IsLoad => true;

    public LwInstruction(int rd, int rs1, int imm) : base(rd, rs1, imm) { }

    public override void Execute(MachineState s, InstructionData d)
    {
        uint addr = (uint)((long)s.Registers.Read(d.Rs1) + (long)(int)d.Immediate);
        uint val = s.Memory!.ReadWord(addr);
        s.Registers.Write(d.Rd, (ulong)(int)val);
    }

    public override void Compute(MachineState state, ulong rs1Val, ulong rs2Val, PipelineBuffers buffers)
    {
        buffers.ExecuteMemory.AluResult = (ulong)((long)rs1Val + (long)buffers.DecodeExecute.Immediate);
    }
}
