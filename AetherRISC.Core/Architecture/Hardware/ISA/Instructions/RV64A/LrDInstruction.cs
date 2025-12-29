using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.Pipeline;

namespace AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64A;

[RiscvInstruction("LR.D", InstructionSet.RV64A, RiscvEncodingType.R, 0x2F, Funct3 = 3, Funct7 = 0x02,
    Name = "Load Reserved Double", Description = "Loads a double word and registers a reservation set.", Usage = "lr.d rd, (rs1)")]
public class LrDInstruction : RTypeInstruction
{
    public LrDInstruction(int rd, int rs1, int rs2 = 0) : base(rd, rs1, rs2) { }
    
    public override void Execute(MachineState s, InstructionData d) {
        uint addr = (uint)s.Registers.Read(d.Rs1);
        s.LoadReservationAddress = addr;
        s.Registers.Write(d.Rd, s.Memory!.ReadDouble(addr));
    }

    public override void Compute(MachineState state, ulong rs1Val, ulong rs2Val, PipelineBuffers buffers)
    {
        buffers.ExecuteMemory.AluResult = rs1Val;
    }
}
