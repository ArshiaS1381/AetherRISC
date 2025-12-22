using AetherRISC.Core.Abstractions.Interfaces;
using AetherRISC.Core.Architecture;

namespace AetherRISC.Core.Hardware.ISA.Base;

public class SwInstruction : IInstruction
{
    public string Mnemonic => "SW";
    public int Rd => 0; // Stores don't write to Rd
    public int Rs1 { get; }
    public int Rs2 { get; }
    public int Imm { get; }

    public bool IsLoad => false;
    public bool IsStore => true;
    public bool IsBranch => false;
    public bool IsJump => false;

    public SwInstruction(int rs1, int rs2, int imm)
    {
        Rs1 = rs1;
        Rs2 = rs2;
        Imm = imm;
    }

    public void Execute(MachineState state)
    {
        ulong baseAddr = state.Registers.Read(Rs1);
        long effectiveAddr = (long)baseAddr + Imm;
        uint value = (uint)state.Registers.Read(Rs2);

        if (state.Memory != null)
        {
            state.Memory.WriteWord((uint)effectiveAddr, value);
        }
        state.ProgramCounter += 4;
    }
}
