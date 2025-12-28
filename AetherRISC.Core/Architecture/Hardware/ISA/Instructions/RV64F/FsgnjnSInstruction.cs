using AetherRISC.Core.Architecture.Hardware.ISA;
using System;
using AetherRISC.Core.Architecture.Simulation.State;
namespace AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64F;

[RiscvInstruction("FSGNJN.S", InstructionSet.RV64F, RiscvEncodingType.R, 0x53, Funct3 = 1, Funct7 = 0x10,
    Name = "Floating-Point Sign Inject Negated (Single)", 
    Description = "Produces a result with the magnitude of rs1 and the opposite sign of rs2.", 
    Usage = "fsgnjn.s fd, fs1, fs2")]
public class FsgnjnSInstruction : RTypeInstruction
{
    public FsgnjnSInstruction(int rd, int rs1, int rs2) : base(rd, rs1, rs2) { }

    public override void Execute(MachineState s, InstructionData d)
    {
        uint b1 = BitConverter.SingleToUInt32Bits(s.FRegisters.ReadSingle(d.Rs1));
        uint b2 = BitConverter.SingleToUInt32Bits(s.FRegisters.ReadSingle(d.Rs2));
        uint res = (b1 & 0x7FFFFFFF) | ((~b2) & 0x80000000);
        s.FRegisters.WriteSingle(d.Rd, BitConverter.UInt32BitsToSingle(res));
    }
}
