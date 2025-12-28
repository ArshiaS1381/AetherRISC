using AetherRISC.Core.Architecture.Hardware.ISA;
using System;
using AetherRISC.Core.Architecture.Simulation.State;
namespace AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64F;

[RiscvInstruction("FLW", InstructionSet.RV64F, RiscvEncodingType.I, 0x07, Funct3 = 2,
    Name = "Load Float Word", 
    Description = "Loads a single-precision value from memory at rs1 + offset into floating-point register rd.", 
    Usage = "flw fd, offset(rs1)")]
public class FlwInstruction : ITypeInstruction
{
    public override bool IsLoad => true;
    public FlwInstruction(int rd, int rs1, int imm) : base(rd, rs1, imm) { }

    public override void Execute(MachineState s, InstructionData d)
    {
        uint addr = (uint)((long)s.Registers.Read(d.Rs1) + (long)(int)d.Immediate);
        uint bits = s.Memory!.ReadWord(addr);
        s.FRegisters.WriteSingle(d.Rd, BitConverter.Int32BitsToSingle((int)bits));
    }
}
