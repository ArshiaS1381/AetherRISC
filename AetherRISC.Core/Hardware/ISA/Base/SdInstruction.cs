using AetherRISC.Core.Abstractions.Interfaces;
using AetherRISC.Core.Architecture;

namespace AetherRISC.Core.Hardware.ISA.Base;

public class SdInstruction : IInstruction
{
    public string Mnemonic => "SD";
    public int Rd => 0;
    public int Rs1 { get; }
    public int Rs2 { get; }
    public int Imm { get; }

    public bool IsLoad => false;
    public bool IsStore => true;
    public bool IsBranch => false;
    public bool IsJump => false;

    public SdInstruction(int rs1, int rs2, int imm) { Rs1 = rs1; Rs2 = rs2; Imm = imm; }
    public void Execute(MachineState state) { /* Logic in Pipeline */ }
}
