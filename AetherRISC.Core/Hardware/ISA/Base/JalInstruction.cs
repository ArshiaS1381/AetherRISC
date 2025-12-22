using AetherRISC.Core.Abstractions.Interfaces;
using AetherRISC.Core.Architecture;

namespace AetherRISC.Core.Hardware.ISA.Base;

public class JalInstruction : IInstruction
{
    public string Mnemonic => "JAL";
    public int Rd { get; }
    public int Rs1 => 0;
    public int Rs2 => 0;
    public int Imm { get; }

    public bool IsLoad => false;
    public bool IsStore => false;
    public bool IsBranch => false;
    public bool IsJump => true;

    public JalInstruction(int rd, int imm)
    {
        Rd = rd;
        Imm = imm;
    }

    public void Execute(MachineState state)
    {
        state.Registers.Write(Rd, state.ProgramCounter + 4);
        state.ProgramCounter = (ulong)((long)state.ProgramCounter + Imm);
    }
}
