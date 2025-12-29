using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.Pipeline;

namespace AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64I;

[RiscvInstruction("SRLW", InstructionSet.RV64I, RiscvEncodingType.R, 0x3B, Funct3 = 5, Funct7 = 0x00,
    Name = "Shift Right Logical Word",
    Description = "Shifts the lower 32 bits of rs1 right by the amount in rs2[4:0], then sign-extends the result to 64 bits.",
    Usage = "srlw rd, rs1, rs2")]
public class SrlwInstruction : RTypeInstruction 
{
    public SrlwInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }

    public override void Execute(MachineState s, InstructionData d) 
    {
        uint v1 = (uint)s.Registers.Read(d.Rs1);
        int shamt = (int)s.Registers.Read(d.Rs2) & 0x1F;
        s.Registers.Write(d.Rd, (ulong)(long)(int)(v1 >> shamt));
    }

    public override void Compute(MachineState state, ulong rs1Val, ulong rs2Val, PipelineBuffers buffers)
    {
        uint v1 = (uint)rs1Val;
        int shamt = (int)rs2Val & 0x1F;
        buffers.ExecuteMemory.AluResult = (ulong)(long)(int)(v1 >> shamt);
    }
}
