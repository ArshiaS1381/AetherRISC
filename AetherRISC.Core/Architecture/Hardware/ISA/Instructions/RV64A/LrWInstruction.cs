using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.Pipeline;

namespace AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64A;

[RiscvInstruction("LR.W", InstructionSet.RV64A, RiscvEncodingType.R, 0x2F, Funct3 = 2, Funct7 = 0x02,
    Name = "Load Reserved Word", Description = "Loads a word and registers a reservation set.", Usage = "lr.w rd, (rs1)")]
public class LrWInstruction : RTypeInstruction
{
    public LrWInstruction(int rd, int rs1, int rs2 = 0) : base(rd, rs1, rs2) { }
    
    public override void Execute(MachineState s, InstructionData d) {
        uint addr = (uint)s.Registers.Read(d.Rs1);
        s.LoadReservationAddress = addr;
        s.Registers.Write(d.Rd, (ulong)(long)(int)s.Memory!.ReadWord(addr));
    }

    public override void Compute(MachineState state, ulong rs1Val, ulong rs2Val, PipelineBuffers buffers)
    {
        // ALU calculates Address (rs1 + 0)
        buffers.ExecuteMemory.AluResult = rs1Val;
        // Reservation handled in Memory Stage via MemRead flag + Inst Type
    }
}
