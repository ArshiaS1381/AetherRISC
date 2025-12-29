using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.Pipeline;

namespace AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64I;

[RiscvInstruction("SUBW", InstructionSet.RV64I, RiscvEncodingType.R, 0x3B, Funct3 = 0, Funct7 = 0x20,
    Name = "Subtract Word",
    Description = "Subtracts the lower 32 bits of rs2 from rs1, truncates to 32 bits, and sign-extends to 64 bits.",
    Usage = "subw rd, rs1, rs2")]
public class SubwInstruction : RTypeInstruction 
{
    public SubwInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }

    public override void Execute(MachineState s, InstructionData d) 
    {
        long res = (long)s.Registers.Read(d.Rs1) - (long)s.Registers.Read(d.Rs2);
        s.Registers.Write(d.Rd, (ulong)(long)(int)res);
    }

    public override void Compute(MachineState state, ulong rs1Val, ulong rs2Val, PipelineBuffers buffers)
    {
        long res = (long)rs1Val - (long)rs2Val;
        buffers.ExecuteMemory.AluResult = (ulong)(long)(int)res;
    }
}
