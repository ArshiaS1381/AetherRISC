using AetherRISC.Core.Abstractions.Interfaces;
using AetherRISC.Core.Architecture;

namespace AetherRISC.Core.Hardware.ISA.Base;

public class BneInstruction : IInstruction
{
    public string Mnemonic => "BNE";
    public int Rd => 0;
    public int Rs1 { get; }
    public int Rs2 { get; }
    public int Imm { get; }

    public bool IsLoad => false;
    public bool IsStore => false;
    public bool IsBranch => true;
    public bool IsJump => false;

    public BneInstruction(int rs1, int rs2, int imm)
    {
        Rs1 = rs1;
        Rs2 = rs2;
        Imm = imm;
    }

    public void Execute(MachineState state)
    {
        ulong val1 = state.Registers.Read(Rs1);
        ulong val2 = state.Registers.Read(Rs2);

        if (val1 != val2)
        {
            long target = (long)state.ProgramCounter + Imm;
            state.ProgramCounter = (ulong)target;
        }
        else
        {
            state.ProgramCounter += 4;
        }
    }
}
