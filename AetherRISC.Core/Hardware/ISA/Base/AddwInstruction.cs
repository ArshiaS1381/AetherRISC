using AetherRISC.Core.Abstractions.Interfaces;
using AetherRISC.Core.Architecture;

namespace AetherRISC.Core.Hardware.ISA.Base;

public class AddwInstruction : IInstruction
{
    public string Mnemonic => "ADDW";
    public int Rd { get; }
    public int Rs1 { get; }
    public int Rs2 { get; }
    public int Imm => 0;

    public bool IsLoad => false;
    public bool IsStore => false;
    public bool IsBranch => false;
    public bool IsJump => false;

    public AddwInstruction(int rd, int rs1, int rs2) { Rd = rd; Rs1 = rs1; Rs2 = rs2; }
    public void Execute(MachineState state) { /* Logic in Pipeline */ }
}
