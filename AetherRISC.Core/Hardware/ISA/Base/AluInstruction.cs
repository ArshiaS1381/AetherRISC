using AetherRISC.Core.Abstractions.Interfaces;
using AetherRISC.Core.Architecture;

namespace AetherRISC.Core.Hardware.ISA.Base;

public enum AluOp { Add, Sub, Sll, Slt, Sltu, Xor, Srl, Sra, Or, And, Mul }

public class AluInstruction : IInstruction
{
    public string Mnemonic { get; }
    public int Rd { get; }
    public int Rs1 { get; }
    public int Rs2 { get; }
    public int Imm => 0;

    public bool IsLoad => false;
    public bool IsStore => false;
    public bool IsBranch => false;
    public bool IsJump => false;

    private readonly AluOp _op; // Kept for metadata

    public AluInstruction(string mnemonic, AluOp op, int rd, int rs1, int rs2)
    {
        Mnemonic = mnemonic;
        _op = op;
        Rd = rd;
        Rs1 = rs1;
        Rs2 = rs2;
    }
    public void Execute(MachineState state) { /* Logic in Pipeline */ }
}
