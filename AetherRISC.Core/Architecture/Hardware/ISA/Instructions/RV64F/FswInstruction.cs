using AetherRISC.Core.Architecture.Hardware.ISA;
using System;
using AetherRISC.Core.Architecture.Simulation.State;
namespace AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64F;

[RiscvInstruction("FSW", InstructionSet.RV64F, RiscvEncodingType.S, 0x27, Funct3 = 2,
    Name = "Store Float Word", 
    Description = "Stores the single-precision value in floating-point register rs2 to memory at rs1 + offset.", 
    Usage = "fsw fs2, offset(rs1)")]
public class FswInstruction : STypeInstruction
{
    public FswInstruction(int rs1, int rs2, int imm) : base(rs1, rs2, imm) { }

    public override void Execute(MachineState s, InstructionData d)
    {
        uint addr = (uint)((long)s.Registers.Read(d.Rs1) + (long)(int)d.Immediate);
        float val = s.FRegisters.ReadSingle(d.Rs2);
        uint bits = unchecked((uint)BitConverter.SingleToInt32Bits(val));
        s.Memory!.WriteWord(addr, bits);
    }
}
