using System;
using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.Pipeline;

namespace AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64D;

[RiscvInstruction("FSD", InstructionSet.RV64D, RiscvEncodingType.S, 0x27, Funct3 = 3,
    Name = "Store Float Double", 
    Description = "Stores a double-precision (64-bit) value from register rs2 to memory at rs1 + offset.", 
    Usage = "fsd fs2, offset(rs1)")]
public class FsdInstruction : STypeInstruction
{
    public FsdInstruction(int rs1, int rs2, int imm) : base(rs1, rs2, imm) { }
    
    public override void Execute(MachineState s, InstructionData d)
    {
        uint addr = (uint)((long)s.Registers.Read(d.Rs1) + (long)(int)d.Immediate);
        double val = s.FRegisters.ReadDouble(d.Rs2);
        s.Memory!.WriteDouble(addr, (ulong)BitConverter.DoubleToInt64Bits(val));
    }

    public override void Compute(MachineState state, ulong rs1Val, ulong rs2Val, PipelineBuffers buffers)
    {
        buffers.ExecuteMemory.AluResult = (ulong)((long)rs1Val + (long)buffers.DecodeExecute.Immediate);
        double val = state.FRegisters.ReadDouble(this.Rs2);
        buffers.ExecuteMemory.StoreValue = (ulong)BitConverter.DoubleToInt64Bits(val);
    }
}
