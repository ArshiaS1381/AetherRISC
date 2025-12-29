using AetherRISC.Core.Architecture.Hardware.ISA;
using System;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.Pipeline;

namespace AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64F;

[RiscvInstruction("FSGNJX.S", InstructionSet.RV64F, RiscvEncodingType.R, 0x53, Funct3 = 2, Funct7 = 0x10,
    Name = "Floating-Point Sign Inject XOR (Single)", 
    Description = "Produces a result with the magnitude of rs1 and a sign equal to the XOR of signs of rs1 and rs2.", 
    Usage = "fsgnjx.s fd, fs1, fs2")]
public class FsgnjxSInstruction : RTypeInstruction
{
    public FsgnjxSInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }

    public override void Execute(MachineState s, InstructionData d)
    {
        uint b1 = BitConverter.SingleToUInt32Bits(s.FRegisters.ReadSingle(d.Rs1));
        uint b2 = BitConverter.SingleToUInt32Bits(s.FRegisters.ReadSingle(d.Rs2));
        uint res = b1 ^ (b2 & 0x80000000);
        s.FRegisters.WriteSingle(d.Rd, BitConverter.UInt32BitsToSingle(res));
    }

    public override void Compute(MachineState state, ulong rs1Val, ulong rs2Val, PipelineBuffers buffers)
    {
        uint b1 = BitConverter.SingleToUInt32Bits(state.FRegisters.ReadSingle(this.Rs1));
        uint b2 = BitConverter.SingleToUInt32Bits(state.FRegisters.ReadSingle(this.Rs2));
        uint res = b1 ^ (b2 & 0x80000000);
        buffers.ExecuteMemory.AluResult = 0xFFFFFFFF00000000 | res;
    }
}
