using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.Pipeline;

namespace AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64I;

[RiscvInstruction("ADDIW", InstructionSet.RV64I, RiscvEncodingType.I, 0x1B, Funct3 = 0,
    Name = "Add Immediate Word",
    Description = "Adds a sign-extended 12-bit immediate to rs1, truncating the result to 32 bits and sign-extending to 64 bits.",
    Usage = "addiw rd, rs1, imm")]
public class AddiwInstruction : ITypeInstruction
{
    public AddiwInstruction(int rd, int rs1, int imm) : base(rd, rs1, imm) { }

    public override void Execute(MachineState s, InstructionData d)
    {
        long res = (long)s.Registers.Read(d.Rs1) + (long)d.Immediate;
        s.Registers.Write(d.Rd, (ulong)(long)(int)res);
    }

    public override void Compute(MachineState state, ulong rs1Val, ulong rs2Val, PipelineBuffers buffers)
    {
        long res = (long)rs1Val + (long)buffers.DecodeExecute.Immediate;
        buffers.ExecuteMemory.AluResult = (ulong)(long)(int)res;
    }
}
