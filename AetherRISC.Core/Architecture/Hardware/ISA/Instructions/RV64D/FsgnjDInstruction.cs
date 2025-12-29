using System;
using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
using AetherRISC.Core.Architecture.Hardware.Pipeline;

namespace AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64D;

[RiscvInstruction("FSGNJ.D", InstructionSet.RV64D, RiscvEncodingType.R, 0x53, Funct3 = 0, Funct7 = 0x11,
    Name = "Sign Inject (Double)", 
    Description = "Magnitude of rs1 with the sign of rs2.", 
    Usage = "fsgnj.d fd, fs1, fs2")]
public class FsgnjDInstruction : RTypeInstruction
{
    public FsgnjDInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }
    
    public override void Execute(MachineState s, InstructionData d)
    {
        ulong b1 = BitConverter.DoubleToUInt64Bits(s.FRegisters.ReadDouble(d.Rs1));
        ulong b2 = BitConverter.DoubleToUInt64Bits(s.FRegisters.ReadDouble(d.Rs2));
        ulong res = (b1 & ~(1UL << 63)) | (b2 & (1UL << 63));
        s.FRegisters.WriteDouble(d.Rd, BitConverter.UInt64BitsToDouble(res));
    }

    public override void Compute(MachineState state, ulong rs1Val, ulong rs2Val, PipelineBuffers buffers)
    {
        ulong b1 = BitConverter.DoubleToUInt64Bits(state.FRegisters.ReadDouble(this.Rs1));
        ulong b2 = BitConverter.DoubleToUInt64Bits(state.FRegisters.ReadDouble(this.Rs2));
        ulong res = (b1 & ~(1UL << 63)) | (b2 & (1UL << 63));
        buffers.ExecuteMemory.AluResult = res;
    }
}
