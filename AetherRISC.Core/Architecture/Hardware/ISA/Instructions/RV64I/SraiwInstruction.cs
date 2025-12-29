using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.Pipeline;

namespace AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64I;

[RiscvInstruction("SRAIW", InstructionSet.RV64I, RiscvEncodingType.ShiftImm, 0x1B, Funct3 = 5, Funct6 = 0x20,
    Name = "Shift Right Arithmetic Immediate Word",
    Description = "Shifts the lower 32 bits of rs1 right by a constant amount. Bits vacated are filled with the 31st bit. Result is sign-extended to 64 bits.",
    Usage = "sraiw rd, rs1, shamt")]
public class SraiwInstruction : ITypeInstruction 
{
    public SraiwInstruction(int rd, int rs1, int imm) : base(rd, rs1, imm) { }

    public override void Execute(MachineState s, InstructionData d) 
    {
        int v1 = (int)s.Registers.Read(d.Rs1);
        int shamt = (int)d.Immediate & 0x1F;
        s.Registers.Write(d.Rd, (ulong)(long)(v1 >> shamt));
    }

    public override void Compute(MachineState state, ulong rs1Val, ulong rs2Val, PipelineBuffers buffers)
    {
        int v1 = (int)rs1Val;
        int shamt = buffers.DecodeExecute.Immediate & 0x1F;
        buffers.ExecuteMemory.AluResult = (ulong)(long)(v1 >> shamt);
    }
}
