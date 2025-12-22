using AetherRISC.Core.Abstractions.Interfaces;
using AetherRISC.Core.Architecture;

namespace AetherRISC.Core.Hardware.ISA.Base;

public class AddiInstruction : IInstruction
{
    public string Mnemonic => "ADDI";
    public int Rd { get; }
    public int Rs1 { get; }
    public int Rs2 => 0; // Unused
    public int Imm { get; }

    public bool IsLoad => false;
    public bool IsStore => false;
    public bool IsBranch => false;
    public bool IsJump => false;

    public AddiInstruction(int rd, int rs1, int imm)
    {
        Rd = rd;
        Rs1 = rs1;
        Imm = imm;
    }

    public void Execute(MachineState state)
    {
        ulong rs1Value = state.Registers.Read(Rs1);
        long result = (long)rs1Value + Imm;
        state.Registers.Write(Rd, (ulong)result);
        state.ProgramCounter += 4;
    }
}
