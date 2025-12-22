using AetherRISC.Core.Abstractions.Interfaces;
using AetherRISC.Core.Architecture;

namespace AetherRISC.Core.Hardware.ISA.Base;

public class NopInstruction : IInstruction
{
    public string Mnemonic => "NOP";
    public int Rd => 0;
    public int Rs1 => 0;
    public int Rs2 => 0;
    public int Imm => 0;
    public bool IsLoad => false;
    public bool IsStore => false;
    public bool IsBranch => false;
    public bool IsJump => false;
    public void Execute(MachineState state) { }
}
