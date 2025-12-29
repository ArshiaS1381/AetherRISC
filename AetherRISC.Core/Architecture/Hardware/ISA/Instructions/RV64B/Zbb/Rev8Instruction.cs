using System.Buffers.Binary;
using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.Pipeline;

namespace AetherRISC.Core.Architecture.Hardware.ISA.Extensions.B.Zbb;

[RiscvInstruction("REV8", InstructionSet.Zbb, RiscvEncodingType.ShiftImm, 0x13, Funct3 = 5, Funct6 = 0x3B,
    Name = "Byte-Reverse", 
    Description = "Reverses the order of bytes in rs1. Essential for endianness conversion.", 
    Usage = "rev8 rd, rs1")]
public class Rev8Instruction : ITypeInstruction
{
    public Rev8Instruction(int rd, int rs1, int imm) : base(rd, rs1, imm) { }

    public override void Execute(MachineState s, InstructionData d)
    {
        ulong val = s.Registers.Read(d.Rs1);
        s.Registers.Write(d.Rd, Calc(val, s.Config.XLEN));
    }

    public override void Compute(MachineState state, ulong rs1Val, ulong rs2Val, PipelineBuffers buffers)
    {
        buffers.ExecuteMemory.AluResult = Calc(rs1Val, state.Config.XLEN);
    }

    private ulong Calc(ulong val, int xlen)
    {
        if (xlen == 32) return (ulong)BinaryPrimitives.ReverseEndianness((uint)val);
        return BinaryPrimitives.ReverseEndianness(val);
    }
}
