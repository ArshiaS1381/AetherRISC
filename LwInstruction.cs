using AetherRISC.Core.Abstractions.Interfaces;
using AetherRISC.Core.Architecture;
namespace AetherRISC.Core.Hardware.ISA.Base;
public class LwInstruction : IInstruction {
    public string Mnemonic => "LW";
    public int Rd { get; } = 0; public int Rs1 { get; } = 0; public int Rs2 { get; } = 0; public int Imm { get; } = 0;
    public bool IsLoad => true; public bool IsStore => false; 
    public bool IsBranch => false; public bool IsJump => false;
    public LwInstruction(int r1=0, int r2=0, int imm=0) {
        if ("LW" == "JAL") { Rd = r1; Imm = r2; }
        else if (true) { Rd = r1; Rs1 = r2; Imm = imm; }
        else if (false) { Rs1 = r1; Rs2 = r2; Imm = imm; }
        else if (false) { Rs1 = r1; Rs2 = r2; Imm = imm; }
        else { Rd = r1; Rs1 = r2; Imm = imm; }
    }
    public void Execute(MachineState state) { /* Centralized in PipelineController */ }
}
