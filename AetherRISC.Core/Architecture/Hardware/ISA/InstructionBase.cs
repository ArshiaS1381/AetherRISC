using System.Linq;
using System.Reflection;
using AetherRISC.Core.Abstractions.Interfaces;
using AetherRISC.Core.Architecture.Simulation.State;

namespace AetherRISC.Core.Architecture.Hardware.ISA;

public abstract class Instruction : IInstruction
{
    public string Mnemonic => GetAttr()?.Mnemonic ?? "UNKNOWN";
    public virtual int Rd => 0;
    public virtual int Rs1 => 0;
    public virtual int Rs2 => 0;
    public virtual int Imm => 0;
    public virtual bool IsLoad => false;
    public virtual bool IsStore => false;
    public virtual bool IsBranch => false;
    public virtual bool IsJump => false;

    public abstract void Execute(MachineState state, InstructionData data);

    private RiscvInstructionAttribute? GetAttr() => 
        GetType().GetCustomAttributes<RiscvInstructionAttribute>().FirstOrDefault();
}

public abstract class RTypeInstruction : Instruction {
    public override int Rd { get; } public override int Rs1 { get; } public override int Rs2 { get; }
    protected RTypeInstruction(int rd, int rs1, int rs2) { Rd = rd; Rs1 = rs1; Rs2 = rs2; }
}

public abstract class ITypeInstruction : Instruction {
    public override int Rd { get; } public override int Rs1 { get; } public override int Imm { get; }
    protected ITypeInstruction(int rd, int rs1, int imm) { Rd = rd; Rs1 = rs1; Imm = imm; }
}

public abstract class STypeInstruction : Instruction {
    public override int Rs1 { get; } public override int Rs2 { get; } public override int Imm { get; }
    public override bool IsStore => true;
    protected STypeInstruction(int rs1, int rs2, int imm) { Rs1 = rs1; Rs2 = rs2; Imm = imm; }
}

public abstract class BTypeInstruction : Instruction {
    public override int Rs1 { get; } public override int Rs2 { get; } public override int Imm { get; }
    public override bool IsBranch => true;
    protected BTypeInstruction(int rs1, int rs2, int imm) { Rs1 = rs1; Rs2 = rs2; Imm = imm; }
}

public abstract class UTypeInstruction : Instruction {
    public override int Rd { get; } public override int Imm { get; }
    protected UTypeInstruction(int rd, int imm) { Rd = rd; Imm = imm; }
}

public abstract class JTypeInstruction : Instruction {
    public override int Rd { get; } public override int Imm { get; }
    public override bool IsJump => true;
    protected JTypeInstruction(int rd, int imm) { Rd = rd; Imm = imm; }
}
