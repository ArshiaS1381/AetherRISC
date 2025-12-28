using AetherRISC.Core.Architecture.Hardware.ISA;
using AetherRISC.Core.Architecture.Simulation.State;
namespace AetherRISC.Core.Architecture.Hardware.ISA.Instructions.RV64I;

[RiscvInstruction("LHU", InstructionSet.RV64I, RiscvEncodingType.I, 0x03, Funct3 = 5,
    Name = "Load Half-word Unsigned",
    Description = "Loads a 16-bit value from memory at rs1 + offset, zero-extends it to 64 bits, and stores it in rd.",
    Usage = "lhu rd, offset(rs1)")]
public class LhuInstruction : ITypeInstruction
{
    public override bool IsLoad => true;

    public LhuInstruction(int rd, int rs1, int imm) : base(rd, rs1, imm) { }

    public override void Execute(MachineState state, InstructionData data)
    {
        uint addr = (uint)((long)state.Registers.Read(data.Rs1) + (long)(int)data.Immediate);
        ushort val = state.Memory!.ReadHalf(addr);
        
        // Zero-extend 16-bit to 64-bit (standard ulong cast behavior)
        state.Registers.Write(data.Rd, (ulong)val);
    }
}
