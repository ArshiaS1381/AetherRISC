using AetherRISC.Core.Abstractions.Interfaces;
using AetherRISC.Core.Architecture;

namespace AetherRISC.Core.Hardware.ISA.Base;

public class JalrInstruction : IInstruction
{
    public string Mnemonic => "JALR";
    public int Rd { get; }
    public int Rs1 { get; }
    public int Rs2 => 0;
    public int Imm { get; }

    public bool IsLoad => false;
    public bool IsStore => false;
    public bool IsBranch => false;
    public bool IsJump => true;

    public JalrInstruction(int rd, int rs1, int imm)
    {
        Rd = rd;
        Rs1 = rs1;
        Imm = imm;
    }

    public void Execute(MachineState state)
    {
        ulong baseVal = state.Registers.Read(Rs1);
        ulong returnAddr = state.ProgramCounter + 4;
        
        long target = (long)baseVal + Imm;
        target &= ~1; 

        state.Registers.Write(Rd, returnAddr);
        state.ProgramCounter = (ulong)target;
    }
}
