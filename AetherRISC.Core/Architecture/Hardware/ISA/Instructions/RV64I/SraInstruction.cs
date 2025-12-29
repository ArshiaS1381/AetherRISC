using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.Pipeline;

namespace AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64I;

[RiscvInstruction("SRA", InstructionSet.RV64I, RiscvEncodingType.R, 0x33, Funct3 = 5, Funct7 = 0x20,
    Name = "Shift Right Arithmetic",
    Description = "Shifts rs1 right by the amount in rs2. The vacated bits are filled with the sign bit of rs1.",
    Usage = "sra rd, rs1, rs2")]
public class SraInstruction : RTypeInstruction 
{
    public SraInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }

    public override void Execute(MachineState s, InstructionData d) 
    {
        int shamtMask = (s.Config.XLEN == 32) ? 0x1F : 0x3F;
        int shamt = (int)s.Registers.Read(d.Rs2) & shamtMask;
        s.Registers.Write(d.Rd, (ulong)((long)s.Registers.Read(d.Rs1) >> shamt));
    }

    public override void Compute(MachineState state, ulong rs1Val, ulong rs2Val, PipelineBuffers buffers)
    {
        int shamtMask = (state.Config.XLEN == 32) ? 0x1F : 0x3F;
        int shamt = (int)rs2Val & shamtMask;
        buffers.ExecuteMemory.AluResult = (ulong)((long)rs1Val >> shamt);
    }
}
