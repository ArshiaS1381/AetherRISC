using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.Pipeline;

namespace AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64I;

[RiscvInstruction("SH", InstructionSet.RV64I, RiscvEncodingType.S, 0x23, Funct3 = 1,
    Name = "Store Half-word",
    Description = "Stores the least-significant 16 bits of rs2 into memory at the address rs1 + offset.",
    Usage = "sh rs2, offset(rs1)")]
public class ShInstruction : STypeInstruction
{
    public ShInstruction(int rs1, int rs2, int imm) : base(rs1, rs2, imm) { }

    public override void Execute(MachineState s, InstructionData d)
    {
        uint addr = (uint)((long)s.Registers.Read(d.Rs1) + (long)(int)d.Immediate);
        ushort value = (ushort)s.Registers.Read(d.Rs2);
        s.Memory!.WriteHalf(addr, value);
    }

    public override void Compute(MachineState state, ulong rs1Val, ulong rs2Val, PipelineBuffers buffers)
    {
        buffers.ExecuteMemory.AluResult = (ulong)((long)rs1Val + (long)buffers.DecodeExecute.Immediate);
        buffers.ExecuteMemory.StoreValue = rs2Val & 0xFFFF;
    }
}
