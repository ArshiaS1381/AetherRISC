using AetherRISC.Core.Abstractions.Interfaces;
using AetherRISC.Core.Architecture;

namespace AetherRISC.Core.Hardware.ISA.Base;

public class LwInstruction : IInstruction
{
    public string Mnemonic => "LW";
    public int Rd { get; }
    public int Rs1 { get; }
    public int Rs2 => 0;
    public int Imm { get; }

    public bool IsLoad => true;
    public bool IsStore => false;
    public bool IsBranch => false;
    public bool IsJump => false;

    public LwInstruction(int rd, int rs1, int imm)
    {
        Rd = rd;
        Rs1 = rs1;
        Imm = imm;
    }

    public void Execute(MachineState state)
    {
        ulong baseAddr = state.Registers.Read(Rs1);
        long effectiveAddr = (long)baseAddr + Imm;

        if (state.Memory != null)
        {
            uint value = state.Memory.ReadWord((uint)effectiveAddr);
            state.Registers.Write(Rd, (ulong)(int)value); // Sign-Extend
        }
        state.ProgramCounter += 4;
    }
}
