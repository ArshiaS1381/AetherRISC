using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.Pipeline;

namespace AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64I;

[RiscvInstruction("SD", InstructionSet.RV64I, RiscvEncodingType.S, 0x23, Funct3 = 3,
    Name = "Store Double-word", 
    Description = "Stores the 64-bit value in rs2 to memory at the address rs1 + offset.", 
    Usage = "sd rs2, offset(rs1)")]
public class SdInstruction : STypeInstruction
{
    public SdInstruction(int rs1, int rs2, int imm) : base(rs1, rs2, imm) { }

    public override void Execute(MachineState s, InstructionData d)
    {
        uint addr = (uint)((long)s.Registers.Read(d.Rs1) + (long)(int)d.Immediate);
        ulong value = s.Registers.Read(d.Rs2);
        s.Memory!.WriteDouble(addr, value);
    }

    public override void Compute(MachineState state, ulong rs1Val, ulong rs2Val, PipelineBuffers buffers)
    {
        buffers.ExecuteMemory.AluResult = (ulong)((long)rs1Val + (long)buffers.DecodeExecute.Immediate);
        buffers.ExecuteMemory.StoreValue = rs2Val;
    }
}
