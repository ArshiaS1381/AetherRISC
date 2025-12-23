using AetherRISC.Core.Abstractions.Interfaces;
using System;
using System.Collections.Concurrent; 

namespace AetherRISC.Core.Architecture.ISA.Encoding;

public static class InstructionEncoder
{
    // Thread-safe dictionary to prevent test crashes
    private static readonly ConcurrentDictionary<string, Func<IInstruction, uint>> _encoders = new();

    public static void Register(string mnemonic, Func<IInstruction, uint> encoder) 
        => _encoders[mnemonic] = encoder;

    public static uint Encode(IInstruction inst)
    {
        if (_encoders.TryGetValue(inst.Mnemonic, out var encoder)) return encoder(inst);
        return 0x00000013; // Default NOP
    }

    // Helpers
    public static uint GenR(uint op, uint f3, uint f7, uint rd, uint rs1, uint rs2)
        => (f7 << 25) | (rs2 << 20) | (rs1 << 15) | (f3 << 12) | (rd << 7) | op;

    public static uint GenI(uint op, uint f3, uint rd, uint rs1, uint imm)
        => ((imm & 0xFFF) << 20) | (rs1 << 15) | (f3 << 12) | (rd << 7) | op;

    public static uint GenS(uint op, uint f3, uint rs1, uint rs2, uint imm)
    {
        uint imm11_5 = (imm >> 5) & 0x7F;
        uint imm4_0 = imm & 0x1F;
        return (imm11_5 << 25) | (rs2 << 20) | (rs1 << 15) | (f3 << 12) | (imm4_0 << 7) | op;
    }

    public static uint GenB(uint op, uint f3, uint rs1, uint rs2, uint imm)
    {
        uint imm12 = (imm >> 12) & 1;
        uint imm10_5 = (imm >> 5) & 0x3F;
        uint imm4_1 = (imm >> 1) & 0xF;
        uint imm11 = (imm >> 11) & 1;
        return (imm12 << 31) | (imm10_5 << 25) | (rs2 << 20) | (rs1 << 15) | (f3 << 12) | (imm4_1 << 8) | (imm11 << 7) | op;
    }

    public static uint GenU(uint op, uint rd, uint imm)
        => (imm & 0xFFFFF000) | (rd << 7) | op;

    public static uint GenJ(uint op, uint rd, uint imm)
    {
        uint imm20 = (imm >> 20) & 1;
        uint imm10_1 = (imm >> 1) & 0x3FF;
        uint imm11 = (imm >> 11) & 1;
        uint imm19_12 = (imm >> 12) & 0xFF;
        return (imm20 << 31) | (imm10_1 << 21) | (imm11 << 20) | (imm19_12 << 12) | (rd << 7) | op;
    }
}
