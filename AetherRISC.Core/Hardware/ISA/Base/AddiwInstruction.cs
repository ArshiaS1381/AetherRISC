using AetherRISC.Core.Abstractions.Interfaces;
using AetherRISC.Core.Architecture;

namespace AetherRISC.Core.Hardware.ISA.Base;

public class AddiwInstruction : IInstruction
{
    public string Mnemonic => "ADDIW";
    public int Rd { get; }
    public int Rs1 { get; }
    public int Rs2 => 0;
    public int Imm { get; }

    public bool IsLoad => false;
    public bool IsStore => false;
    public bool IsBranch => false;
    public bool IsJump => false;

    public AddiwInstruction(int rd, int rs1, int imm) { Rd = rd; Rs1 = rs1; Imm = imm; }
    public void Execute(MachineState state) { /* Logic in Pipeline */ }
}
