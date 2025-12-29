using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.Pipeline;

namespace AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64A;

[RiscvInstruction("SC.W", InstructionSet.RV64A, RiscvEncodingType.R, 0x2F, Funct3 = 2, Funct7 = 0x03,
    Name = "Store Conditional Word", Description = "Conditionally stores a word if the reservation is still valid.", Usage = "sc.w rd, rs2, (rs1)")]
public class ScWInstruction : RTypeInstruction
{
    public ScWInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }
    
    public override void Execute(MachineState s, InstructionData d) {
        uint addr = (uint)s.Registers.Read(d.Rs1);
        bool success = false;
        if (s.LoadReservationAddress.HasValue && s.LoadReservationAddress.Value == addr) {
            s.Memory!.WriteWord(addr, (uint)s.Registers.Read(d.Rs2));
            success = true;
            s.LoadReservationAddress = null;
        }
        s.Registers.Write(d.Rd, success ? 0UL : 1UL);
    }

    public override void Compute(MachineState state, ulong rs1Val, ulong rs2Val, PipelineBuffers buffers)
    {
        buffers.ExecuteMemory.AluResult = rs1Val; // Address
        buffers.ExecuteMemory.StoreValue = rs2Val;
    }
}
