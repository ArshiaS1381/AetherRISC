using AetherRISC.Core.Abstractions.Interfaces;
using AetherRISC.Core.Hardware.ISA.Base;

namespace AetherRISC.Core.Helpers;

public static class Inst {
    public static IInstruction Nop() => new NopInstruction();
    // --- 64-bit Math ---
    public static IInstruction Addi(int rd, int rs1, int imm) => new AddiInstruction(rd, rs1, imm);
    public static IInstruction Add(int rd, int rs1, int rs2)  => new AluInstruction("ADD", AluOp.Add, rd, rs1, rs2);
    public static IInstruction Sub(int rd, int rs1, int rs2)  => new AluInstruction("SUB", AluOp.Sub, rd, rs1, rs2);
    public static IInstruction Slt(int rd, int rs1, int rs2)  => new AluInstruction("SLT", AluOp.Slt, rd, rs1, rs2);
    
    // NEW: Multiplication
    public static IInstruction Mul(int rd, int rs1, int rs2) => new AluInstruction("MUL", AluOp.Mul, rd, rs1, rs2);

    // --- 32-bit Math ---
    public static IInstruction Addiw(int rd, int rs1, int imm) => new AddiwInstruction(rd, rs1, imm);
    public static IInstruction Addw(int rd, int rs1, int rs2)  => new AddwInstruction(rd, rs1, rs2);

    // --- Memory ---
    public static IInstruction Lw(int rd, int rs1, int imm)   => new LwInstruction(rd, rs1, imm);
    public static IInstruction Sw(int rs1, int rs2, int imm)  => new SwInstruction(rs1, rs2, imm);
    public static IInstruction Ld(int rd, int rs1, int imm)   => new LdInstruction(rd, rs1, imm);
    public static IInstruction Sd(int rs1, int rs2, int imm)  => new SdInstruction(rs1, rs2, imm);

    // --- Control Flow ---
    public static IInstruction Bne(int rs1, int rs2, int imm) => new BneInstruction(rs1, rs2, imm);
    
    // NEW: Jumps (Missing definitions fixed here)
    public static IInstruction Jal(int rd, int imm) => new JalInstruction(rd, imm);
    public static IInstruction Jalr(int rd, int rs1, int imm) => new JalrInstruction(rd, rs1, imm);
    
    public static IInstruction Ecall() => new EcallInstruction();
    public static IInstruction Lui(int rd, int imm) => new LuiInstruction(rd, imm);
}

