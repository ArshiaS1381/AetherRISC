using AetherRISC.Core.Abstractions.Interfaces;
using AetherRISC.Core.Architecture;

namespace AetherRISC.Core.Hardware.ISA.Base;

public class LdInstruction : IInstruction
{
    public string Mnemonic => "LD";
    public int Rd { get; }
    public int Rs1 { get; }
    public int Rs2 => 0;
    public int Imm { get; }

    public bool IsLoad => true;
    public bool IsStore => false;
    public bool IsBranch => false;
    public bool IsJump => false;

    public LdInstruction(int rd, int rs1, int imm) { Rd = rd; Rs1 = rs1; Imm = imm; }
    public void Execute(MachineState state) { /* Logic in Pipeline */ }
}
