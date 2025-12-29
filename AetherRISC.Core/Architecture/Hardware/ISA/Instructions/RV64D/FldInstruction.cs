using AetherRISC.Core.Architecture.Hardware.ISA;
using System;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.Pipeline;

namespace AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64D;

[RiscvInstruction("FLD", InstructionSet.RV64D, RiscvEncodingType.I, 0x07, Funct3 = 3,
    Name = "Load Float Double", 
    Description = "Loads a double-precision (64-bit) value from memory into floating-point register rd.", 
    Usage = "fld fd, offset(rs1)")]
public class FldInstruction : ITypeInstruction
{
    public override bool IsLoad => true;
    public FldInstruction(int rd, int rs1, int imm) : base(rd, rs1, imm) { }

    public override void Execute(MachineState s, InstructionData d)
    {
        uint addr = (uint)((long)s.Registers.Read(d.Rs1) + (long)(int)d.Immediate);
        s.FRegisters.WriteRaw(d.Rd, s.Memory!.ReadDouble(addr));
    }

    public override void Compute(MachineState state, ulong rs1Val, ulong rs2Val, PipelineBuffers buffers)
    {
        buffers.ExecuteMemory.AluResult = (ulong)((long)rs1Val + (long)buffers.DecodeExecute.Immediate);
    }
}
