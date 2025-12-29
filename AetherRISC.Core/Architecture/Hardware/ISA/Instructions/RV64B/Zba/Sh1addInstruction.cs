using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.Pipeline;

namespace AetherRISC.Core.Architecture.Hardware.ISA.Extensions.B.Zba;

[RiscvInstruction("SH1ADD", InstructionSet.Zba, RiscvEncodingType.R, 0x33, Funct3 = 2, Funct7 = 0x10,
    Name = "Shift Left by 1 and Add", 
    Description = "Shifts rs1 left by 1 and adds it to rs2. Useful for half-word array indexing.", 
    Usage = "sh1add rd, rs1, rs2")]
public class Sh1addInstruction : RTypeInstruction
{
    public Sh1addInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }

    public override void Execute(MachineState s, InstructionData d) =>
        s.Registers.Write(d.Rd, (s.Registers.Read(d.Rs1) << 1) + s.Registers.Read(d.Rs2));

    public override void Compute(MachineState state, ulong rs1Val, ulong rs2Val, PipelineBuffers buffers)
    {
        buffers.ExecuteMemory.AluResult = (rs1Val << 1) + rs2Val;
    }
}
